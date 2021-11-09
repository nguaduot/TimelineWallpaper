﻿using TimelineWallpaper.Beans;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.Data.Html;
using TimelineWallpaper.Utils;

namespace TimelineWallpaper.Providers {
    public class NasaProvider : BaseProvider {
        // 下一页数据索引（日期编号）（用于按需加载）
        private DateTime nextPage = DateTime.UtcNow.AddHours(-4);

        // https://api.nasa.gov/
        // Query Parameters
        // date: The date of the APOD image to retrieve
        // start_date: The start of a date range, when requesting date for a range of dates. Cannot be used with "date".
        // end_date: The end of the date range, when used with "start_date".
        // count: If this is specified then "count" randomly chosen images will be returned. Cannot be used with "date" or "start_date" and "end_date".
        // thumbs: Return the URL of video thumbnail. If an APOD is not a video, this parameter is ignored.
        // api_key: api.nasa.gov key for expanded usage
        private const string URL_API_PAGE = "https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY&thumbs=True&start_date={0}&end_date={1}";

        public NasaProvider() {
            Id = "nasa";
        }

        private Meta ParseBean(NasaApiItem item) {
            Meta meta = new Meta {
                Title = item.Title,
                Story = item.Explanation
            };
            if ("image".Equals(item.MediaType)) {
                meta.Uhd = item.HdUrl;
                meta.Thumb = item.Url;
                meta.Format = item.HdUrl.Substring(item.HdUrl.LastIndexOf("."));
            }/* else if ("video".Equals(item.MediaType)) { // 放弃，被墙的URL加载不出来
                meta.Video = item.Url;
                meta.Thumb = item.ThumbnailUrl;
            }*/
            if (!string.IsNullOrEmpty(item.Date)) {
                meta.Date = DateTime.ParseExact(item.Date, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
                meta.Id = item.MediaType + meta.Date?.ToString("yyyyMMdd");
            }
            if (!string.IsNullOrEmpty(item.Copyright)) {
                meta.Copyright = "© " + item.Copyright.Replace("\n", "").Replace(" Music:", "");
            }

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

            string urlApi = string.Format(URL_API_PAGE, nextPage.AddDays(-6).ToString("yyyy-MM-dd"), nextPage.ToString("yyyy-MM-dd"));
            nextPage = nextPage.AddDays(-7);
            Debug.WriteLine("provider url: " + urlApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(urlApi);
                Debug.WriteLine("provider data: " + jsonData);
                List<NasaApiItem> items = JsonConvert.DeserializeObject<List<NasaApiItem>>(jsonData);
                items.Sort((a, b) => b.Date.CompareTo(a.Date));
                foreach (NasaApiItem item in items) {
                    Meta meta = ParseBean(item);
                    if (!meta.IsValid()) {
                        continue;
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

    public class NasabjpProvider : BaseProvider {
        // 下一页数据索引（从0开始）（用于按需加载）
        private int nextPage = 0;

        private readonly List<string> pageUrls = new List<string>();

        private const string URL_API_HOST = "http://www.bjp.org.cn";
        private const string URL_API_TODAY = URL_API_HOST + "/mryt/list.shtml";
        private const string URL_API_DAY = URL_API_HOST + "/mryt/list_{0}.shtml";

        public NasabjpProvider() {
            Id = "nasa";
        }

        private void ParsePages(string html) {
            foreach (Match match in Regex.Matches(html, @">([\d-]+)：<.+?href=""(.+?)""")) {
                pageUrls.Add(URL_API_HOST + match.Groups[2].Value);
            }
        }

        private Meta ParseBean(string htmlData) {
            Meta meta = new Meta();
            Match match = Regex.Match(htmlData, @"contentid ?= ?""(.+?)"";");
            if (match.Success) {
                meta.Id = match.Groups[1].Value;
            }
            match = Regex.Match(htmlData, @"<img src=""(.+?(\..+?))""");
            if (match.Success) {
                meta.Uhd = URL_API_HOST + match.Groups[1].Value;
                meta.Thumb = URL_API_HOST + match.Groups[1].Value;
                meta.Format = match.Groups[2].Value;
            }
            match = Regex.Match(htmlData, @"<source src=""(.+?(\..+?))""");
            if (match.Success) {
                meta.Video = URL_API_HOST + match.Groups[1].Value;
                meta.Format = match.Groups[2].Value;
            }
            match = Regex.Match(htmlData, @"<strong>([^=]+?)<br ?/>(.+?)</p>");
            if (match.Success) {
                meta.Title = HtmlUtilities.ConvertToText(match.Groups[1].Value);
                meta.Copyright = HtmlUtilities.ConvertToText(match.Groups[2].Value).Trim(new char[] { '\n', ' ' });
            }
            match = Regex.Match(htmlData, @">说明：(.+?)</p>");
            if (match.Success) {
                meta.Story = HtmlUtilities.ConvertToText(match.Groups[1].Value).Trim(new char[] { '\n', ' ' }) + "（翻译：北京天文馆）";
            }
            match = Regex.Match(htmlData, @">(\d+\-\d+\-\d+)<");
            if (match.Success) {
                meta.Date = DateTime.ParseExact(match.Groups[1].Value, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"));
            }
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

            if (nextPage >= pageUrls.Count) {
                string urlBjp = nextPage >= 100 ? string.Format(URL_API_DAY, (int)Math.Ceiling((nextPage + 1) / 100.0)) : URL_API_TODAY;
                try {
                    HttpClient client = new HttpClient();
                    string htmlData = await client.GetStringAsync(urlBjp);
                    ParsePages(htmlData);
                } catch (Exception e) {
                    Debug.WriteLine(e);
                }
            }
            if (nextPage >= pageUrls.Count) {
                return metas.Count > 0;
            }

            string url = pageUrls[nextPage++];
            try {
                HttpClient client = new HttpClient();
                string htmlData = await client.GetStringAsync(url);
                Meta meta = ParseBean(htmlData);
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

    //public class NasaProvider2 : BaseProvider {
    //    // 下一页数据索引（日期编号）（用于按需加载）
    //    private DateTime nextPage = DateTime.Now;

    //    private const string URL_API_HOST = "https://apod.nasa.gov/apod/";
    //    private const string URL_API_TODAY = URL_API_HOST + "astropix.html";
    //    // 注意时差导致404
    //    private const string URL_API_DAY = URL_API_HOST + "ap{0}.html";

    //    public NasaProvider2() {
    //        Id = "nasa";
    //    }

    //    private Meta ParseBean(string htmlData) {
    //        Meta meta = new Meta();
    //        Match match = Regex.Match(htmlData, @"href=""(ap(\d{6}))\.html""> ?&lt; ?</a>");
    //        if (match.Success) {
    //            DateTime dateYesterday = DateTime.ParseExact(match.Groups[2].Value, "yyMMdd",
    //                new System.Globalization.CultureInfo("en-US"));
    //            meta.Id = match.Groups[1].Value;
    //            meta.Date = dateYesterday.AddDays(1);
    //        }
    //        if (meta.Id == null || meta.Date == null) {
    //            return null;
    //        }
    //        match = Regex.Match(htmlData, @"href=""(image.+?)""");
    //        if (match.Success) {
    //            meta.Uhd = URL_API_HOST + match.Groups[1].Value;
    //        }
    //        match = Regex.Match(htmlData, @"SRC=""(image.+?)""");
    //        if (match.Success) {
    //            meta.Thumb = URL_API_HOST + match.Groups[1].Value;
    //        }
    //        match = Regex.Match(htmlData, @"<b>(.+?)</b> ?<br>");
    //        if (match.Success) {
    //            meta.Title = match.Groups[1].Value.Trim();
    //        }
    //        match = Regex.Match(htmlData, @"(<b> *?Image Credit.+?)</center>", RegexOptions.Singleline);
    //        if (match.Success) {
    //            string line = HtmlUtilities.ConvertToText(match.Groups[1].Value);
    //            Debug.WriteLine(line);
    //            meta.Copyright = "© " + line.Split(":", 2)[1].Split(";")[0].Trim();
    //        } else {
    //            match = Regex.Match(htmlData, @"(<b>.+?Images:.+?)</center>", RegexOptions.Singleline);
    //            if (match.Success) {
    //                string line = HtmlUtilities.ConvertToText(match.Groups[1].Value);
    //                Debug.WriteLine(line);
    //                meta.Copyright = "© " + line.Split("Images:", 2)[1].Split(";")[0].Trim();
    //            }
    //        }
    //        match = Regex.Match(htmlData, @"<b> ?Explanation: ?</b>(.+?)<p>", RegexOptions.Singleline);
    //        if (match.Success) {
    //            meta.Story = HtmlUtilities.ConvertToText(match.Groups[1].Value).Trim();
    //        }

    //        return meta;
    //    }

    //    public override async Task<bool> LoadData(Ini ini) {
    //        // 现有数据未浏览完，无需加载更多，或已无更多数据
    //        if (indexFocus + 1 < metas.Count) {
    //            return true;
    //        }
    //        // 无网络连接
    //        if (!NetworkInterface.GetIsNetworkAvailable()) {
    //            return false;
    //        }

    //        string url = nextPage.DayOfYear == DateTime.Now.DayOfYear ? URL_API_TODAY
    //            : string.Format(URL_API_DAY, nextPage.ToString("yyMMdd"));
    //        nextPage = nextPage.AddDays(-1);
    //        try {
    //            HttpClient client = new HttpClient();
    //            string htmlData = await client.GetStringAsync(url);
    //            Meta metaNew = ParseBean(htmlData);
    //            if (metaNew == null) {
    //                return metas.Count > 0;
    //            }
    //            bool exists = false;
    //            foreach (Meta meta in metas) {
    //                exists |= metaNew.Id == meta.Id;
    //            }
    //            if (!exists) {
    //                metas.Add(metaNew);
    //            }
    //            if (nextPage.DayOfYear == metaNew.Date.DayOfYear) { // 解决时差问题
    //                nextPage = nextPage.AddDays(-1);
    //            }
    //        } catch (Exception e) {
    //            Debug.WriteLine(e);
    //        }

    //        return metas.Count > 0;
    //    }
    //}

    //public class NasachinaProvider : BaseProvider {
    //    // 下一页数据索引（从1开始）（用于按需加载）
    //    private int nextPage = 1;

    //    private const string URL_API = "https://www.nasachina.cn/apod/page/{0}";

    //    public NasachinaProvider() {
    //        Id = "nasachina";
    //    }

    //    private List<Meta> ParseBeans(string htmlData) {
    //        List<Meta> metas = new List<Meta>();
    //        foreach (Match m in Regex.Matches(htmlData, @"<article.+?</article>", RegexOptions.Singleline)) {
    //            Meta meta = new Meta();
    //            Match match = Regex.Match(m.Value, @"id=""(.+?)""");
    //            if (match.Success) {
    //                meta.Id = match.Groups[1].Value;
    //            }
    //            match = Regex.Match(m.Value, @"published.+?datetime=""(.+?)T");
    //            if (match.Success) {
    //                meta.Date = Convert.ToDateTime(match.Groups[1].Value);
    //            }
    //            if (meta.Id == null || meta.Date == null) {
    //                continue;
    //            }
    //            //match = Regex.Match(m.Value, @"srcset=""(.+?)""");
    //            //if (match.Success) { // TODO
    //            //    string[] urls = match.Groups[1].Value.Split(",");
    //            //    Array.Sort(urls, (p1, p2) => p2.Split(" ")[1].CompareTo(p1.Split(" ")[1]));
    //            //    Debug.WriteLine(string.Join(", ", urls));
    //            //    meta.Uhd = urls[0].Trim().Split(" ")[0];
    //            //    meta.Thumb = urls[urls.Length - 1].Trim().Split(" ")[0];
    //            //}
    //            match = Regex.Match(m.Value, @"src=""(.+?)""");
    //            if (match.Success) {
    //                meta.Thumb = match.Groups[1].Value;
    //                meta.Uhd = match.Groups[1].Value; // TODO
    //            }
    //            match = Regex.Match(m.Value, @"<h2.+?</h2>", RegexOptions.Singleline);
    //            if (match.Success) {
    //                meta.Title = HtmlUtilities.ConvertToText(match.Value).Trim();
    //            }
    //            metas.Add(meta);
    //        }

    //        return metas;
    //    }

    //    public override async Task<bool> LoadData(Ini ini) {
    //        // 现有数据未浏览完，无需加载更多，或已无更多数据
    //        if (indexFocus + 1 < metas.Count) {
    //            return true;
    //        }
    //        // 无网络连接
    //        if (!NetworkInterface.GetIsNetworkAvailable()) {
    //            return false;
    //        }

    //        string url = string.Format(URL_API, nextPage++);
    //        Debug.WriteLine("provider url: " + url);
    //        try {
    //            HttpClient client = new HttpClient();
    //            string htmlData = await client.GetStringAsync(url);
    //            //Debug.WriteLine("provider data: " + htmlData);
    //            List<Meta> metasNew = ParseBeans(htmlData);
    //            foreach (Meta metaNew in metasNew) {
    //                bool exists = false;
    //                foreach (Meta meta in metas) {
    //                    exists |= metaNew.Id == meta.Id;
    //                }
    //                if (!exists) {
    //                    metas.Add(metaNew);
    //                }
    //            }
    //        } catch (Exception e) {
    //            Debug.WriteLine(e);
    //        }

    //        return metas.Count > 0;
    //    }
    //}
}