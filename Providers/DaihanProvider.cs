using TimelineWallpaper.Beans;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;
using System.Text.RegularExpressions;

namespace TimelineWallpaper.Providers {
    public class DaihanProvider : BaseProvider {
        // 随机二次元ACG图片-呆憨API
        // https://api.daihan.top/html/acg.html
        private const string URL_API = "https://api.daihan.top/api/acg/index.php";

        public DaihanProvider() {
            Id = "daihan";
        }

        private Meta ParseBean(Uri uriImg) {
            Meta meta = new Meta();
            if (uriImg == null) {
                return meta;
            }
            string[] name = uriImg.Segments[uriImg.Segments.Length - 1].Split(".");
            meta.Id = name[0];
            meta.Format = "." + name[1];
            meta.Uhd = Regex.Replace(uriImg.AbsoluteUri, @"(?<=\.sinaimg\.cn/)[^/]+", "large");
            meta.Thumb = Regex.Replace(uriImg.AbsoluteUri, @"(?<=\.sinaimg\.cn/)[^/]+", "middle");
            meta.Date = DateTime.Now;
            return meta;
        }

        public override async Task<bool> LoadData(Ini ini) {
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus + 1 < metas.Count) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            Debug.WriteLine("provider url: " + URL_API);
            try {
                HttpClient client = new HttpClient(new HttpClientHandler {
                    AllowAutoRedirect = false
                });
                HttpResponseMessage msg = await client.GetAsync(URL_API);
                Meta meta = ParseBean(msg.Headers.Location);
                if (!meta.IsValid()) {
                    return metas.Count > 0;
                }
                bool exists = false;
                foreach (Meta m in metas) {
                    exists |= meta.Id.Equals(m.Id);
                }
                if (!exists) {
                    metas.Add(meta);
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
