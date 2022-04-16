using TimelineWallpaper.Beans;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace TimelineWallpaper.Providers {
    public class LofterProvider : BaseProvider {
        // Lofter 登录页图片
        private const string URL_API = "https://www.lofter.com/front/login";

        private string FindJson(string html) {
            if (string.IsNullOrEmpty(html)) {
                return "{}";
            }
            Match match = Regex.Match(html, @"""loginPageData"":(\{.+?\}),""loginType"":");
            return match.Success ? match.Groups[1].Value : "{}";
        }

        private Meta ParseBean(LofterApiBg bean, long curDate) {
            Meta meta = new Meta {
                Date = DateUtil.FromUnixMillis(curDate),
                Copyright = "@" + bean.Author.Name
            };
            meta.SortFactor = meta.Date.Value.Subtract(new DateTime(1970, 1, 1)).Days;
            if (bean.Image != null) {
                meta.Id = bean.Image;
                meta.Uhd = bean.Image.Split("?")[0];
                meta.Thumb = bean.Image;
            }/* else if (bean.Video != null) {
                meta.Id = bean.Video;
                meta.Video = bean.Video.Split("?")[0];
            }*/
            Match match = Regex.Match(meta.Id, @"https://lofter.lf127.net/(.+?)/[^\.]+(\.[^\?]+)");
            if (match.Success) {
                meta.Id = match.Groups[1].Value;
                meta.Format = match.Groups[2].Value;
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
            await base.LoadData(ini, date);

            Debug.WriteLine("provider url: " + URL_API);
            try {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("timelinewallpaper", VerUtil.GetPkgVer(true)));
                string htmlData = await client.GetStringAsync(URL_API);
                //Debug.WriteLine("provider data: " + htmlData);
                LofterApi lofterApi = JsonConvert.DeserializeObject<LofterApi>(FindJson(htmlData));
                List<Meta> metasAdd = new List<Meta>();
                foreach (LofterApiBg bg in lofterApi.Background) {
                    metasAdd.Add(ParseBean(bg, lofterApi.CurDate));
                }
                RandomMetas(metasAdd);
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
