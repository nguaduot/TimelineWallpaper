using TimelineWallpaper.Beans;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;
using System.Net;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using Windows.Storage;
using System.Drawing;

namespace TimelineWallpaper.Providers {
    public class Himawari8Provider : BaseProvider {
        // 下一页索引（从0开始）（用于按需加载）
        private DateTime nextPage = DateTime.UtcNow.AddMinutes(-15).AddMinutes(-DateTime.UtcNow.AddMinutes(-15).Minute % 10);
        
        // 向日葵-8号即時網頁 - NICT
        // https://himawari8.nict.go.jp/zh/himawari8-image.htm
        private const string URL_API = "https://himawari8.nict.go.jp/img/D531106/1d/550/{0}/{1}_0_0.png";

        private Meta ParseBean(DateTime time) {
            string index = string.Format("{0}{1}000", time.ToString("HH"), (time.Minute / 10));
            Meta meta = new Meta {
                Id = time.ToString("yyyyMMdd") + index,
                Uhd = string.Format(URL_API, time.ToString(@"yyyy\/MM\/dd"), index),
                Format = ".png"
            };
            meta.Thumb = meta.Uhd;
            meta.Date = time.ToLocalTime();
            meta.Caption = meta.Date.Value.ToString("M") + " " + meta.Date.Value.ToString("t");
            return meta;
        }

        public override async Task<bool> LoadData(BaseIni ini, DateTime? date = null) {
            // 无需加载更多
            if (indexFocus < metas.Count - 1) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            try {
                for (int i = 0; i < 5; i++) {
                    string urlApi = string.Format(URL_API,
                        nextPage.AddMinutes(-10 * i).ToString(@"yyyy\/MM\/dd"),
                        string.Format("{0}{1}000", nextPage.AddMinutes(-10 * i).ToString("HH"),
                        (nextPage.AddMinutes(-10 * i).Minute / 10)));
                    Debug.WriteLine("provider url: " + urlApi);
                    HttpWebRequest req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(urlApi));
                    req.Method = HttpMethod.Head.Method;
                    var res = (HttpWebResponse)await req.GetResponseAsync();
                    Debug.WriteLine(res.ContentLength);
                    if (res.StatusCode == HttpStatusCode.OK && res.ContentLength > 10 * 1024) {
                        Meta meta = ParseBean(nextPage);
                        bool exists = false;
                        foreach (Meta m in metas) {
                            exists |= meta.Id.Equals(m.Id);
                        }
                        if (!exists) {
                            metas.Add(meta);
                        }
                        break;
                    }
                    res.Close();
                }
                SortMetas();
                nextPage = nextPage.AddHours(-1);
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
            return metas.Count > 0;
        }

        public override async Task<Meta> Cache(Meta meta) {
            _ = await base.Cache(meta);
            if (meta == null || !meta.IsCached() || meta.CacheUhd.Path.Contains("-reset")) {
                return meta;
            }

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasBitmap bitmap = null;
            using (var stream = await meta.CacheUhd.OpenReadAsync()) {
                bitmap = await CanvasBitmap.LoadAsync(device, stream);
            }
            if (bitmap == null) {
                return meta;
            }
            meta.Dimen = new Size(1920, 1080);
            CanvasRenderTarget target = new CanvasRenderTarget(device, meta.Dimen.Width, meta.Dimen.Height, 96);
            using (var session = target.CreateDrawingSession()) {
                session.Clear(Colors.Black);
                session.DrawImage(bitmap, (meta.Dimen.Width - bitmap.SizeInPixels.Width) / 2,
                    (meta.Dimen.Height - bitmap.SizeInPixels.Height) / 2);
            }

            meta.CacheUhd = await ApplicationData.Current.TemporaryFolder
                .CreateFileAsync(Id + "-" + meta.Id + "-reset" + meta.Format, CreationCollisionOption.OpenIfExists);
            await target.SaveAsync(meta.CacheUhd.Path, CanvasBitmapFileFormat.Png);
            return meta;
        }
    }
}
