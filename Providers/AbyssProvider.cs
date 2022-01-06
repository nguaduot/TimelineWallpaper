using TimelineWallpaper.Beans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TimelineWallpaper.Utils;
using Windows.Data.Html;

namespace TimelineWallpaper.Providers {
    public class AbyssProvider : BaseProvider {
        // 页数据索引（从1开始）（用于按需加载）
        private int pageIndex = 0;

        // 近期受欢迎的壁纸（30张每页）
        // https://wall.alphacoders.com/popular.php?lang=Chinese
        private const string URL_API = "https://wall.alphacoders.com/popular.php?page={0}{1}";

        private List<Meta> ParseBeans(string htmlData) {
            List<Meta> metas = new List<Meta>();
            foreach (Match m in Regex.Matches(htmlData, @"<div class=[""']thumb\-container[""'].+?class=[""']tags-info[""']", RegexOptions.Singleline)) {
                Meta meta = new Meta {
                    Date = DateTime.Now // 需从详情页获取
                };
                Match match = Regex.Match(m.Groups[0].Value, @"src=[""']([^""]+thumbbig-(\d+)(\.[^""']+))[""']", RegexOptions.Singleline);
                if (match.Success) {
                    meta.Id = match.Groups[2].Value;
                    meta.Thumb = match.Groups[1].Value;
                    meta.Format = match.Groups[3].Value;
                }
                meta.Uhd = meta.Thumb.Replace("thumbbig-", "");
                match = Regex.Match(m.Groups[0].Value, @">([^<]+)</a>&nbsp;\-&nbsp;[^>]+>([^<]+)</a>", RegexOptions.Singleline);
                if (match.Success) {
                    meta.Title = match.Groups[1].Value.Trim() + " - " + match.Groups[2].Value.Trim();
                }
                match = Regex.Match(m.Groups[0].Value, @"btn\-user.+?>([^<>]+)</span>", RegexOptions.Singleline);
                if (match.Success) {
                    meta.Copyright = "@" + match.Groups[1].Value;
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

            ++pageIndex;
            string lang = "";
            if ("zh".Equals(System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName)) {
                lang = "&lang=Chinese";
            }
            string url = string.Format(URL_API, pageIndex, lang);
            Debug.WriteLine("provider url: " + url);
            try {
                HttpClient client = new HttpClient();
                string htmlData = await client.GetStringAsync(url);
                //Debug.WriteLine("provider data: " + htmlData);
                List<Meta> metasAdd = ParseBeans(htmlData);
                RandomMetas(metasAdd);
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
