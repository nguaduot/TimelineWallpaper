using Newtonsoft.Json;

namespace TimelineWallpaper.Beans {
    public class NasaApiItem {
        // 日期
        [JsonProperty(PropertyName = "date")]
        public string Date { set; get; }

        // 作者&版权（可能缺失）
        [JsonProperty(PropertyName = "copyright")]
        public string Copyright { set; get; }

        // 标题
        [JsonProperty(PropertyName = "title")]
        public string Title { set; get; }

        // 描述
        [JsonProperty(PropertyName = "explanation")]
        public string Explanation { set; get; }

        // 媒体类型
        [JsonProperty(PropertyName = "media_type")]
        public string MediaType { set; get; }

        // 原图URL（media_type为“video”时缺失）
        [JsonProperty(PropertyName = "hdurl")]
        public string HdUrl { set; get; }

        // 缩略图URL（media_type为“video”时是视频链接）
        [JsonProperty(PropertyName = "url")]
        public string Url { set; get; }

        // 视频封面URL（media_type非“video”时缺失）
        [JsonProperty(PropertyName = "thumbnail_url")]
        public string ThumbnailUrl { set; get; }

        // ...
    }
}
