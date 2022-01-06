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
    public class MtyProvider : BaseProvider {
        // 墨天逸 - 随机图片API
        // https://api.mtyqx.cn/
        private const string URL_API = "https://api.mtyqx.cn/api/random.php?return=json";

        private Meta ParseBean(DmoeApiItem bean) {
            Meta meta = new Meta();
            if (bean?.ImgUrl == null) {
                return meta;
            }
            // 若直接使用字符串需反转义 Regex.Unescape()
            // https:\/\/tva2.sinaimg.cn\/large\/0075auPSly1fqb5xmdoa4j31jk0rzds0.jpg
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

            Debug.WriteLine("provider url: " + URL_API);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(URL_API);
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
