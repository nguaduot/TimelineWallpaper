using TimelineWallpaper.Beans;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;

namespace TimelineWallpaper.Providers {
    public class CoolapkProvider : BaseProvider {
        // 下一页数据索引（从0开始）（用于按需加载）
        private int nextPage = 0;

        private const string URL_API = "http://180.76.116.163/timeline?group=酷安&enddate={0}&order={1}";

        public CoolapkProvider() {
            Id = "coolapk";
        }

        private Meta ParseBean(CoolapkApiData bean) {
            Meta meta = new Meta {
                Id = bean?.Id.ToString(),
                Uhd = bean.ImgUrl,
                Thumb = bean.ThumbUrl,
                Title = bean.Title,
                Story = bean.Story,
                Copyright = "@" + bean.Author,
                Date = DateTime.ParseExact(bean.RelDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")),
            };
            if (bean.ImgUrl != null) {
                Uri uri = new Uri(bean.ImgUrl);
                string[] name = uri.Segments[uri.Segments.Length - 1].Split(".");
                meta.Format = "." + name[1];
            } else {
                meta.Format = ".jpg";
            }

            return meta;
        }

        public override async Task<bool> LoadData(Ini ini) {
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus + 1 < metas.Count || nextPage++ > 0) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            string urlApi = string.Format(URL_API, DateTime.Now.ToString("yyyyMMdd"), ini.Coolapk.Order);
            Debug.WriteLine("provider url: " + urlApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(urlApi);
                Debug.WriteLine("provider data: " + jsonData);
                CoolapkApi coolapkApi = JsonConvert.DeserializeObject<CoolapkApi>(jsonData);
                foreach (CoolapkApiData item in coolapkApi.Data) {
                    Meta meta = ParseBean(item);
                    if (!meta.IsValid()) {
                        continue;
                    }
                    bool exists = false;
                    foreach (Meta m in metas) {
                        exists |= meta.Id.Equals(m.Id);
                    }
                    if (!exists) {
                        metas.Add(meta);
                    }
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
            return metas.Count > 0;
        }
    }
}
