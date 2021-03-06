using TimelineWallpaper.Beans;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace TimelineWallpaper.Providers {
    public class DmoeProvider : BaseProvider {
        // 随机二次元图片API - 樱花
        // https://www.dmoe.cc/
        private const string URL_API = "https://www.dmoe.cc/random.php?return=json&t={0}";

        private Meta ParseBean(DmoeApiItem bean) {
            Meta meta = new Meta();
            if (bean?.ImgUrl == null) {
                return meta;
            }
            // 若直接使用字符串需反转义 Regex.Unescape()
            // https:\/\/tva1.sinaimg.cn\/large\/0072Vf1pgy1foxk7r8ic6j31hc0u0k7b.jpg
            Uri uri = new Uri(bean.ImgUrl);
            string[] name = uri.Segments[uri.Segments.Length - 1].Split(".");
            meta.Id = name[0];
            meta.Format = "." + name[1];
            meta.Uhd = Regex.Replace(uri.AbsoluteUri, @"(?<=\.sinaimg\.cn/)[^/]+", "large");
            meta.Thumb = Regex.Replace(uri.AbsoluteUri, @"(?<=\.sinaimg\.cn/)[^/]+", "middle");
            meta.Date = DateTime.Now;
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
            await base.LoadData(ini, date);

            string url = string.Format(URL_API, DateUtil.CurrentTimeMillis());
            Debug.WriteLine("provider url: " + url);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(url);
                Debug.WriteLine("provider data: " + jsonData.Trim());
                DmoeApiItem item = JsonConvert.DeserializeObject<DmoeApiItem>(jsonData);
                List<Meta> metasAdd = new List<Meta> {
                    ParseBean(item)
                };
                AppendMetas(metasAdd);
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
