using TimelineWallpaper.Beans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TimelineWallpaper.Utils;

namespace TimelineWallpaper.Providers {
    public class G3Provider : BaseProvider {
        // 页数据索引（从1开始）（用于按需加载）
        private int pageIndex = 0;

        // 最新壁纸
        private const string URL_API_SORT1 = "https://desk.3gbizhi.com/index_{0}.html";
        // 热门壁纸
        private const string URL_API_SORT2 = "https://desk.3gbizhi.com";

        private List<Meta> ParseBeans(string htmlData, bool dataHot) {
            List<Meta> metas = new List<Meta>();
            if (dataHot) {
                Match match = Regex.Match(htmlData, @">热门壁纸推荐<.+?</ul>", RegexOptions.Singleline);
                if (!match.Success) {
                    return metas;
                }
                htmlData = match.Value;
            }
            foreach (Match m in Regex.Matches(htmlData, @"<img lazysrc=""(.+?)""[^>]+?title=""(.+?)"" ?/>")) {
                Meta meta = new Meta {
                    Thumb = m.Groups[1].Value,
                    Caption = m.Groups[2].Value
                };
                Match match = Regex.Match(m.Groups[1].Value, @"https://pic.3gbizhi.com/(\d{4}/\d{4})/(\d+)(\.[^.]+)");
                if (match.Success) {
                    meta.Id = match.Groups[2].Value;
                    meta.Uhd = match.Value;
                    meta.Date = DateTime.ParseExact(match.Groups[1].Value, "yyyy/MMdd", new System.Globalization.CultureInfo("en-US"));
                    meta.Format = match.Groups[3].Value;
                }
                meta.SortFactor = meta.Date.Value.Subtract(new DateTime(1970, 1, 1)).Days;
                match = Regex.Match(m.Groups[2].Value, @"^(.+?)(?:的)?(?:美图桌面|高清桌面|唯美桌面|桌面|唯美场景|唯美插画|唯美风光景色|创意动漫3D|高清海报|摄影|酷飒游戏|酷飒|搞怪|艺术|唯美|手绘插画|油墨风插画|场景插画|人物插画|插画|高清|风景|动漫)?(?:壁纸)?图片$");
                if (!match.Success) {
                    match = Regex.Match(m.Groups[2].Value, @"^(.+?)(?:的)?(?:唯美艺术山水画|唯美摄影|唯美意境|唯美|影视插画|插画|静态摄影|静态|创意手绘|个性|创意|玄幻|可爱|霸气|帅气|美图|静态摄影|近距离摄影|唯美摄影|室外摄影|摄影|浪漫|超清.*?|高清.*?)?(?:高清)?(?:手机)?(?:电脑)?(?:桌面)?壁纸$");
                    if (!match.Success) {
                        match = Regex.Match(m.Groups[2].Value, @"^(.+?)(?:桌面.*?下载|静态.*?下载|高清.*?下载|超清.*?下载|插画.*?下载|高清.*?张|唯美动漫壁纸图|唯美创意壁纸美图|壁纸美图|美图|壁纸图)$");
                    }
                }
                meta.Title = match.Success ? match.Groups[1].Value.Trim() : m.Groups[2].Value;
                metas.Add(meta);
            }
            return metas;
        }

        public override async Task<bool> LoadData(BaseIni ini, DateTime? date = null) {
            bool sortByHot = "view".Equals(((G3Ini)ini).Order);
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus < metas.Count - 1 || (sortByHot && pageIndex > 0)) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }
            await base.LoadData(ini, date);

            ++pageIndex;
            string url = sortByHot ? URL_API_SORT2 : string.Format(URL_API_SORT1, pageIndex);
            Debug.WriteLine("provider url: " + url);
            try {
                HttpClient client = new HttpClient();
                string htmlData = await client.GetStringAsync(url);
                //Debug.WriteLine("provider data: " + htmlData);
                List<Meta> metasAdd = ParseBeans(htmlData, sortByHot);
                if (sortByHot) {
                    RandomMetas(metasAdd);
                } else {
                    AppendMetas(metasAdd);
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
