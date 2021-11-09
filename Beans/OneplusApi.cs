using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class OneplusApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "data")]
        public List<OneplusApiItem> Items { set; get; }
    }

    public class OneplusApiItem {
        // ID
        [JsonProperty(PropertyName = "photoCode")]
        public string PhotoCode { set; get; }

        // URL
        [JsonProperty(PropertyName = "photoUrl")]
        public string PhotoUrl { set; get; }

        // 日期
        [JsonProperty(PropertyName = "scheduleTime")]
        public string ScheduleTime { set; get; }

        // 作者
        [JsonProperty(PropertyName = "author")]
        public string Author { set; get; }

        // 标题
        [JsonProperty(PropertyName = "photoTopic")]
        public string PhotoTopic { set; get; }

        // 描述
        [JsonProperty(PropertyName = "remark")]
        public string Remark { set; get; }

        // 国家
        [JsonProperty(PropertyName = "countryCodeStr")]
        public string CountryCodeStr { set; get; }

        // 地点
        [JsonProperty(PropertyName = "photoLocation")]
        public string PhotoLocation { set; get; }

        // ...
    }

    public class OneplusRequest {
        [JsonProperty(PropertyName = "pageSize")]
        public int PageSize { set; get; }

        [JsonProperty(PropertyName = "currentPage")]
        public int CurrentPage { set; get; }

        [JsonProperty(PropertyName = "sortMethod")]
        public string SortMethod { set; get; }
    }
}
