using TimelineWallpaper.Beans;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;
using System.Collections.Generic;

namespace TimelineWallpaper.Providers {
    public class SeovxProvider : BaseProvider {
        // 在线古风美图二次元API - 夏沫博客
        // https://cdn.seovx.com/
        private const string URL_API = "https://cdn.seovx.com/{0}/?mom=302";

        private Meta ParseBean(Uri uriImg) {
            Meta meta = new Meta();
            if (uriImg == null) {
                return meta;
            }
            if (uriImg.ToString().StartsWith("//")) {
                uriImg = new Uri("https:" + uriImg.ToString());
            }
            string[] name = uriImg.Segments[uriImg.Segments.Length - 1].Split(".");
            meta.Id = name[0];
            meta.Format = "." + name[1];
            meta.Uhd = uriImg.AbsoluteUri;
            meta.Thumb = uriImg.AbsoluteUri;
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

            string uriApi = string.Format(URL_API, ((SeovxIni)ini).Cate);
            Debug.WriteLine("provider url: " + uriApi);
            try {
                HttpClient client = new HttpClient(new HttpClientHandler {
                    AllowAutoRedirect = false
                });
                HttpResponseMessage msg = await client.GetAsync(uriApi);
                List<Meta> metasAdd = new List<Meta> {
                    ParseBean(msg.Headers.Location)
                };
                AppendMetas(metasAdd);
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
