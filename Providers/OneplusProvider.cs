using TimelineWallpaper.Beans;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

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
                Title = bean.PhotoTopic?.Trim(),
                Copyright = "@" + bean.Author,
                Date = DateTime.ParseExact(bean.ScheduleTime, "yyyyMMdd", new System.Globalization.CultureInfo("en-US"))
            };
            meta.SortFactor = meta.Date.Value.Subtract(new DateTime(1970, 1, 1)).TotalDays;

            if (!bean.PhotoTopic.Equals(bean.Remark?.Trim())) {
                meta.Caption = bean.Remark?.Trim();
            }
            if (!bean.PhotoTopic.Equals(bean.PhotoLocation?.Trim())) {
                meta.Location = bean.PhotoLocation?.Trim();
            }
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
            await base.LoadData(ini, date);

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
                HttpRequestMessage msgReq = new HttpRequestMessage(HttpMethod.Post, URL_API);
                //msgReq.Headers.Add("Cookie", "LOCALE=zh_CN; Path=/");
                msgReq.Content = new StringContent(requestStr, Encoding.UTF8, "application/json");
                HttpResponseMessage msgRes = await client.SendAsync(msgReq);
                _ = msgRes.EnsureSuccessStatusCode();
                string jsonData = await msgRes.Content.ReadAsStringAsync();
                Debug.WriteLine("provider data: " + jsonData.Trim());
                OneplusApi oneplusApi = JsonConvert.DeserializeObject<OneplusApi>(jsonData);
                List<Meta> metasAdd = new List<Meta>();
                foreach (OneplusApiItem item in oneplusApi.Items) {
                    metasAdd.Add(ParseBean(item));
                }
                if ("date".Equals(((OneplusIni)ini).Order)) { // 按时序倒序排列
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
