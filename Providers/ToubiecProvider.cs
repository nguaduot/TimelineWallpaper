using TimelineWallpaper.Beans;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Providers {
    public class ToubiecProvider : BaseProvider {
        // 随机二次元图片接口 - 晓晴
        // https://acg.toubiec.cn/
        private const string URL_API = "https://acg.toubiec.cn/random.php?ret=json";

        public ToubiecProvider() {
            Id = "toubiec";
        }

        private Meta ParseBean(ToubiecApiItem bean) {
            Meta meta = new Meta {
                Id = bean?.Id.ToString(),
            };
            if (bean?.ImgUrl == null) {
                return meta;
            }
            Uri uri = new Uri(bean.ImgUrl);
            meta.Uhd = uri.AbsoluteUri.Replace(".sinaimg.cn/large/", ".sinaimg.cn/original/");
            meta.Thumb = uri.AbsoluteUri.Replace(".sinaimg.cn/large/", ".sinaimg.cn/middle/");
            meta.Date = DateTime.Now;
            string[] name = uri.Segments[uri.Segments.Length - 1].Split(".");
            meta.Format = "." + name[1];
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
                List<ToubiecApiItem> toubbiecApi = JsonConvert.DeserializeObject<List<ToubiecApiItem>>(jsonData);
                foreach (ToubiecApiItem item in toubbiecApi) {
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
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
