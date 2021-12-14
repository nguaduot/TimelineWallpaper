using TimelineWallpaper.Beans;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TimelineWallpaper.Providers {
    public class InfinityProvider : BaseProvider {
        // 页数据索引（从0开始）（用于按需加载）
        private int pageIndex = -1;

        // Infinity新标签页 - 壁纸库
        // http://cn.infinitynewtab.com/
        private const string URL_API = "https://infinity-api.infinitynewtab.com/get-wallpaper?source=&tag=&order=1&page={0}";
        private const string URL_API_RANDOM = "https://infinity-api.infinitynewtab.com/random-wallpaper?_={0}";

        private Meta ParseBean(InfinityApiData bean) {
            Meta meta = new Meta {
                Id = bean.Id,
                Uhd = bean.Src?.RawSrc,
                Thumb = bean.Src?.SmallSrc,
                Story = string.Join(" ", bean.Tags ?? new List<string>()),
                Copyright = "© " + bean.Source,
                Date = DateTime.Now,
                Format = ".jpg"
            };

            if (!string.IsNullOrEmpty(bean.Src?.RawSrc)) {
                Uri uri = new Uri(bean.Src.RawSrc);
                string[] nameSuffix = uri.Segments[uri.Segments.Length - 1].Split(".");
                meta.Format = nameSuffix.Length > 1 ? "." + nameSuffix[nameSuffix.Length - 1] : ".jpg";
            }
            return meta;
        }

        public override async Task<bool> LoadData(BaseIni ini, DateTime? date = null) {
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus < metas.Count - 1) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            string urlApi = "rate".Equals(((InfinityIni)ini).Order) ? String.Format(URL_API, ++pageIndex)
                : string.Format(URL_API_RANDOM, DateUtil.CurrentTimeMillis());
            Debug.WriteLine("provider url: " + urlApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(urlApi);
                Debug.WriteLine("provider data: " + jsonData.Trim());
                InfinityApi infinityApi = JsonConvert.DeserializeObject<InfinityApi>(jsonData);
                foreach (InfinityApiData item in infinityApi.Data) {
                    Meta meta = ParseBean(item);
                    if (!meta.IsValid()) {
                        continue;
                    }
                    bool exists = false;
                    foreach (Meta m in metas) {
                        exists |= meta.Id.Equals(m.Id);
                    }
                    if (!exists) {
                        metas.Add(ParseBean(item));
                    }
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
