using TimelineWallpaper.Beans;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;

namespace TimelineWallpaper.Providers {
    public class OneProvider : BaseProvider {
        // 下一页数据索引（从0开始）（用于按需加载）
        private string nextPage = "0";

        private string cookie = null;
        private string token = null;

        // 图文 - 「ONE · 一个」
        // http://m.wufazhuce.com/one
        private const string URL_TOKEN = "http://m.wufazhuce.com/one";
        private const string URL_API = "http://m.wufazhuce.com/one/ajaxlist/{0}?_token={1}";

        private Meta ParseBean(OneApiData bean) {
            Meta meta = new Meta {
                Id = bean.Id,
                Uhd = bean.ImgUrl,
                Thumb = bean.ImgUrl,
                Title = bean.Title,
                Story = bean.Content,
                Copyright = bean.PictureAuthor,
                Date = DateTime.ParseExact(bean.Date, "yyyy / MM / dd", new System.Globalization.CultureInfo("en-US")),
            };
            meta.SortFactor = meta.Date.Value.Subtract(new DateTime(1970, 1, 1)).Days;
            if (!string.IsNullOrEmpty(bean.Content)) {
                meta.Title = "";
                foreach (Match match in Regex.Matches(bean.Content, @"([^  ，、。！？；：(?:——)\n(?:\r\n)]+)([  ，、。！？；：(?:——)\n(?:\r\n)])")) {
                    meta.Title += match.Groups[1].Value;
                    if (meta.Title.Length < 6) {
                        meta.Title += match.Groups[2].Value;
                    } else {
                        if (meta.Title.Length > 16) {
                            meta.Title = meta.Title.Substring(0, 16);
                        }
                        break;
                    }
                }
                meta.Title += "……";
            }
            if (!string.IsNullOrEmpty(bean.TextAuthors)) {
                meta.Story += "\n——" + bean.TextAuthors;
            }
            meta.Format = ".jpg";

            return meta;
        }

        public override async Task<bool> LoadData(BaseIni ini, DateTime? date = null) {
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus < metas.Count - 1 && date == null) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }
            await base.LoadData(ini, date);

            if (string.IsNullOrEmpty(cookie) || string.IsNullOrEmpty(token)) {
                try {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage msg = await client.GetAsync(URL_TOKEN);
                    cookie = new List<string>(msg.Headers.GetValues("Set-Cookie"))[0];
                    Debug.WriteLine("cookie: " + cookie);
                    string htmlData = await msg.Content.ReadAsStringAsync();
                    Match match = Regex.Match(htmlData, @"One.token ?= ?[""'](.+?)[""']");
                    if (match.Success) {
                        token = match.Groups[1].Value;
                        Debug.WriteLine("token: " + token);
                    }
                } catch (Exception e) {
                    Debug.WriteLine(e);
                }
            }
            if (string.IsNullOrEmpty(cookie) || string.IsNullOrEmpty(token)) {
                return metas.Count > 0;
            }

            if ("random".Equals(((OneIni)ini).Order)) {
                nextPage = (3012 + new Random().Next((DateTime.Now - DateTime.Parse("2020-11-10")).Days)).ToString();
            } else {
                nextPage = metas.Count > 0 ? metas[metas.Count - 1].Id : "0";
            }
            string urlApi = string.Format(URL_API, nextPage, token);
            Debug.WriteLine("provider url: " + urlApi);
            try {
                HttpClientHandler handler = new HttpClientHandler() {
                    UseCookies = false
                };
                HttpClient client = new HttpClient(handler);
                HttpRequestMessage msgReq = new HttpRequestMessage(HttpMethod.Get, urlApi);
                msgReq.Headers.Add("Cookie", cookie);
                HttpResponseMessage msgRes = await client.SendAsync(msgReq);
                string jsonData = await msgRes.Content.ReadAsStringAsync();
                Debug.WriteLine("provider data: " + jsonData.Trim());
                OneApi oneApi = JsonConvert.DeserializeObject<OneApi>(jsonData);
                List<Meta> metasAdd = new List<Meta>();
                foreach (OneApiData item in oneApi.Data) {
                    metasAdd.Add(ParseBean(item));
                }
                if ("date".Equals(((OneIni)ini).Order)) { // 按时序倒序排列
                    SortMetas(metasAdd);
                } else {
                    RandomMetas(metasAdd);
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
            return metas.Count > 0;
        }
    }
}
