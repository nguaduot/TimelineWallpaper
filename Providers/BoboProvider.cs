using TimelineWallpaper.Beans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TimelineWallpaper.Utils;
using System.Globalization;

namespace TimelineWallpaper.Providers {
    public class BoboProvider : BaseProvider {
        // 页数据索引（从1开始）（用于按需加载）
        private int pageIndex = 0;

        private readonly List<int> pages = new List<int>() {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39
        };

        // BoBoPic | 每天都有好看的壁纸图片
        // BoBoPic | 4K
        // https://bobopic.com/
        private const string URL_API = "https://bobopic.com/category/4k/page/{0}";
        private const string URL_UHD = "https://dl.bobopic.com/tu/{0}{1}";

        private List<Meta> ParseBeans(string htmlData) {
            List<Meta> metas = new List<Meta>();
            foreach (Match m in Regex.Matches(htmlData, @"<article.+?</article>", RegexOptions.Singleline)) {
                Meta meta = new Meta();
                Match match = Regex.Match(m.Groups[0].Value, @"srcset=""([^""]+/small/(\d+)(\.[^""]+))""");
                if (match.Success) {
                    meta.Id = match.Groups[2].Value;
                    meta.Thumb = match.Groups[1].Value;
                    meta.Format = match.Groups[3].Value;
                }
                meta.Uhd = string.Format(URL_UHD, meta.Id, meta.Format);
                //meta.Uhd = meta.Thumb.Replace("/small/", "/tu/");
                match = Regex.Match(m.Groups[0].Value, @"<time>(.+?)</time>");
                if (match.Success) {
                    DateTime.TryParseExact("2021-" + match.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal, out DateTime date);
                    meta.Date = date;
                }
                match = Regex.Match(m.Groups[0].Value, @"""bookmark"">([^<]+)</a>");
                if (match.Success) {
                    meta.Caption = match.Groups[1].Value;
                    meta.Title = Regex.Replace(meta.Caption, @"(的|超清)?4K(插画|壁纸|人物插画|壁纸插画)(图片)?$", "");
                }
                match = Regex.Match(m.Groups[0].Value, @"插画师：(.+?)(?:Pixiv id：|Pid：)", RegexOptions.Singleline);
                if (match.Success) {
                    meta.Copyright = "@" + match.Groups[1].Value.Trim();
                }
                metas.Add(meta);
            }
            return metas;
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

            pageIndex = pages[new Random().Next(pages.Count)];
            string url = string.Format(URL_API, pageIndex);
            Debug.WriteLine("provider url: " + url);
            try {
                HttpClient client = new HttpClient();
                string htmlData = await client.GetStringAsync(url);
                //Debug.WriteLine("provider data: " + htmlData);
                List<Meta> metasAdd = ParseBeans(htmlData);
                RandomMetas(metasAdd);
                pages.Remove(pageIndex);
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
