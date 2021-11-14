using TimelineWallpaper.Beans;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;

namespace TimelineWallpaper.Providers {
    public class DmoeProvider : BaseProvider {
        // 随机二次元图片API - 樱花
        // https://www.dmoe.cc/
        private const string URL_API = "https://www.dmoe.cc/random.php?return=json";

        public DmoeProvider() {
            Id = "dmoe";
        }

        private Meta ParseBean(DmoeApiItem bean) {
            Meta meta = new Meta();
            if (bean?.ImgUrl == null) {
                return meta;
            }
            Uri uri = new Uri(bean.ImgUrl);
            string[] name = uri.Segments[uri.Segments.Length - 1].Split(".");
            meta.Id = name[0];
            meta.Format = "." + name[1];
            meta.Uhd = uri.AbsoluteUri.Replace(".sinaimg.cn/large/", ".sinaimg.cn/original/");
            meta.Thumb = uri.AbsoluteUri.Replace(".sinaimg.cn/large/", ".sinaimg.cn/middle/");
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
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(URL_API);
                Debug.WriteLine("provider data: " + jsonData);
                DmoeApiItem item = JsonConvert.DeserializeObject<DmoeApiItem>(jsonData);
                Meta meta = ParseBean(item);
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
