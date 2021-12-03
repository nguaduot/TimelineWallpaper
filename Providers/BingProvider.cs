﻿using TimelineWallpaper.Beans;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TimelineWallpaper.Providers {
    public class BingProvider : BaseProvider {
        // 下一页数据索引（从0开始）（用于按需加载）
        private int nextPage = 0;

        // http://s.cn.bing.net
        // https://cn.bing.com
        private const string URL_API_HOST = "https://global.bing.com";
        // Bing搜索 官方提供的 API
        // GET 参数：
        // pid: hp（缺省会导致数据不全）
        // format：hp-HTML，js-JSON，xml/其他值-XML（默认）
        // ensearch：国际版置1，国内版置0（默认）
        // setmkt: HOST为“global.bing.com”时生效，zh-cn、en-us、ja-jp、de-de、fr-fr
        // idx：回溯天数，0为当天，-1为明天，1为昨天，依次类推（最大值7）
        // n：返回数据的条数。若为2，会包含前一天的数据，依次类推（最大值8）
        // uhd：0-1920x1080，1-uhdwidth/uhdheight生效
        // uhdwidth：uhd置1时生效，最大值3840
        // uhdheight：uhd置1时生效
        // API 第三方文档：http://www.lib4dev.in/info/facefruit/daily-bing-wallpaper/209719167
        // 语言代码表：http://www.lingoes.net/zh/translator/langcode.htm
        private readonly string[] URL_API_PAGES = new string[] {
            URL_API_HOST + "/HPImageArchive.aspx?pid=hp&format=js&uhd=1&idx=0&n=8",
            URL_API_HOST + "/HPImageArchive.aspx?pid=hp&format=js&uhd=1&idx=7&n=8",
        };

        public BingProvider() {
            Id = ProviderBing.ID;
        }

        private Meta ParseBean(BingApiImg bean) {
            Meta meta = new Meta {
                Id = bean.Hsh,
                Uhd = string.Format("{0}{1}_UHD.jpg", URL_API_HOST, bean.UrlBase),
                Thumb = string.Format("{0}{1}_400x240.jpg", URL_API_HOST, bean.UrlBase),
                Date = DateTime.ParseExact(bean.EndDate, "yyyyMMdd", new System.Globalization.CultureInfo("en-US")),
                Caption = bean.Copyright,
                Format = ".jpg"
            };

            if (!string.IsNullOrEmpty(bean.Title)) {
                if (!bean.Title.Equals("Info")) { // ko-kr等未支持的地区
                    meta.Title = bean.Title;
                }
            }
            if (!string.IsNullOrEmpty(bean.Desc)) {
                meta.Story = bean.Desc;
            }

            // zh-cn: 正爬上唐娜·诺克沙滩的灰海豹，英格兰北林肯郡 (© Frederic Desmette/Minden Pictures)
            // en-us: Aerial view of the island of Mainau on Lake Constance, Germany (© Amazing Aerial Agency/Offset by Shutterstock)
            // ja-jp: ｢ドナヌックのハイイロアザラシ｣英国, ノースリンカーンシャー (© Frederic Desmette/Minden Pictures)
            Match match = Regex.Match(meta.Caption, @"(.+)[\(（]©(.+)[\)）]");
            if (match.Success) {
                meta.Caption = match.Groups[1].Value.Trim();
                meta.Copyright = "© " + match.Groups[2].Value.Trim();
                match = Regex.Match(meta.Caption, @"｢(.+)｣(.+)");
                if (match.Success) { // 国内版（日本）
                    meta.Caption = match.Groups[1].Value.Trim();
                    meta.Location = match.Groups[2].Value.Trim();
                } else { // 国内版（中国）
                    match = Regex.Match(meta.Caption, @"(.+)[，](.+)");
                    if (match.Success) {
                        meta.Caption = match.Groups[1].Value.Trim();
                        meta.Location = match.Groups[2].Value.Trim();
                    }
                }
            }

            return meta;
        }

        public override async Task<bool> LoadData(Ini ini) {
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus + 1 < metas.Count || nextPage >= URL_API_PAGES.Length) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            string urlApi = URL_API_PAGES[nextPage++];
            if (ini.Bing.Lang.Length > 0) {
                urlApi += "&setmkt=" + ini.Bing.Lang;
            }
            Debug.WriteLine("provider url: " + urlApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(urlApi);
                Debug.WriteLine("provider data: " + jsonData);
                BingApi bingApi = JsonConvert.DeserializeObject<BingApi>(jsonData);
                foreach (BingApiImg img in bingApi.Images) {
                    Meta meta = ParseBean(img);
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
}
