﻿using TimelineWallpaper.Beans;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace TimelineWallpaper.Providers {
    public class OneplusProvider : BaseProvider {
        // 页数据索引（从1开始）（用于按需加载）
        private int pageIndex = 0;

        private const int PAGE_SIZE = 99;

        private const string URL_API = "https://photos.oneplus.com/cn/shot/photo/schedule";

        private Meta ParseBean(OneplusApiItem bean) {
            Meta meta = new Meta {
                Id = bean.PhotoCode,
                Uhd = bean.PhotoUrl,
                Thumb = bean.PhotoUrl.Replace(".jpg", "_400_0.jpg"),
                Title = bean.PhotoTopic,
                Caption = bean.Remark,
                Location = bean.PhotoLocation,
                Copyright = "@" + bean.Author,
                Date = DateTime.ParseExact(bean.ScheduleTime, "yyyyMMdd", new System.Globalization.CultureInfo("en-US")),
                Format = ".jpg"
            };

            if (!string.IsNullOrEmpty(bean.CountryCodeStr)) {
                meta.Copyright += " | " + bean.CountryCodeStr;
            }

            return meta;
        }

        public override async Task<bool> LoadData(BaseIni ini, DateTime? date = null) {
            // 现有数据未浏览完，无需加载更多
            if (date == null || GetFarthest() == null || date.Value.Date >= GetFarthest().Date.Value.Date) {
                if (indexFocus < metas.Count - 1) {
                    return true;
                }
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            // "1"：最新添加，"2"：点赞最多，"3"：浏览最多
            string sort = "rate".Equals(((OneplusIni)ini).Order) ? "2" : ("view".Equals(((OneplusIni)ini).Order) ? "3" : "1");
            OneplusRequest request = new OneplusRequest {
                PageSize = PAGE_SIZE, // 不限
                CurrentPage = ++pageIndex,
                SortMethod = sort
            };
            string requestStr = JsonConvert.SerializeObject(request);
            Debug.WriteLine("provider url: " + URL_API + " " + requestStr);
            try {
                HttpClient client = new HttpClient();
                HttpContent content = new StringContent(requestStr);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync(URL_API, content);
                _ = response.EnsureSuccessStatusCode();
                string jsonData = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("provider data: " + jsonData.Trim());
                OneplusApi oneplusApi = JsonConvert.DeserializeObject<OneplusApi>(jsonData);
                foreach (OneplusApiItem item in oneplusApi.Items) {
                    Debug.WriteLine(item.ScheduleTime);
                    Meta meta = ParseBean(item);
                    if (!meta.IsValid()) {
                        continue;
                    }
                    bool exists = false;
                    foreach (Meta m in metas) {
                        exists |= meta.Id.Equals(m.Id);
                    }
                    if (!exists) {
                        metas.Add(ParseBean(item));
                    }
                }
                if ("1".Equals(((OneplusIni)ini).Order)) { // 按时序倒序排列
                    SortMetas();
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
