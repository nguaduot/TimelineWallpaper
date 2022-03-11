using TimelineWallpaper.Beans;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;
using System.Collections.Generic;

namespace TimelineWallpaper.Providers {
    public class TimelineProvider : BaseProvider {
        // 下一页数据索引（从0开始）（用于按需加载）
        private DateTime nextPage = DateTime.UtcNow.AddHours(8);

        // 自建图源
        // https://github.com/nguaduot/TimelineApi
        private const string URL_API = "https://api.nguaduot.cn/timeline?client=timelinewallpaper&cate={0}&enddate={1}&order={2}&authorize={3}";
        
        private Meta ParseBean(TimelineApiData bean) {
            Meta meta = new Meta {
                Id = bean.Id.ToString(),
                Uhd = bean.ImgUrl,
                Thumb = bean.ThumbUrl,
                Title = bean.Title,
                Cate = bean.Cate,
                Story = bean.Story?.Trim(),
                Copyright = "@" + bean.Author?.Trim(),
                Date = DateTime.ParseExact(bean.RelDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")),
            };
            if (bean.Deprecated != 0) {
                meta.Title = "🚫 " + meta.Title;
            }
            if (!string.IsNullOrEmpty(bean.Platform)) {
                meta.Copyright = bean.Platform + meta.Copyright;
            }
            if (bean.ImgUrl != null) {
                Uri uri = new Uri(bean.ImgUrl);
                string[] name = uri.Segments[uri.Segments.Length - 1].Split(".");
                meta.Format = "." + name[1];
            } else {
                meta.Format = ".jpg";
            }

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

            nextPage = date ?? nextPage;
            string urlApi = string.Format(URL_API, ((TimelineIni)ini).Cate,
                nextPage.ToString("yyyyMMdd"), ((TimelineIni)ini).Order, ((TimelineIni)ini).Authorize);
            Debug.WriteLine("provider url: " + urlApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(urlApi);
                Debug.WriteLine("provider data: " + jsonData.Trim());
                TimelineApi timelineApi = JsonConvert.DeserializeObject<TimelineApi>(jsonData);
                List<Meta> metasAdd = new List<Meta>();
                foreach (TimelineApiData item in timelineApi.Data) {
                    metasAdd.Add(ParseBean(item));
                }
                if ("date".Equals(((TimelineIni)ini).Order)) { // 按时序倒序排列
                    SortMetas(metasAdd);
                } else {
                    AppendMetas(metasAdd);
                }
                nextPage = "date".Equals(((TimelineIni)ini).Order)
                    ? nextPage.AddDays(-timelineApi.Data.Count) : nextPage;
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
            return metas.Count > 0;
        }
    }
}
