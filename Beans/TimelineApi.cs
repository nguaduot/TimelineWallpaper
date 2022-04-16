using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class TimelineApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "data")]
        public List<TimelineApiData> Data { set; get; }
    }

    public class TimelineApiData {
        // ID
        [JsonProperty(PropertyName = "id")]
        public int Id { set; get; }

        // 图片URL
        [JsonProperty(PropertyName = "imgurl")]
        public string ImgUrl { set; get; }

        // 缩略图URL
        [JsonProperty(PropertyName = "thumburl")]
        public string ThumbUrl { set; get; }

        // 标题
        [JsonProperty(PropertyName = "title")]
        public string Title { set; get; }

        // 图文故事
        [JsonProperty(PropertyName = "story")]
        public string Story { set; get; }

        // 出处
        [JsonProperty(PropertyName = "platform")]
        public string Platform { set; get; }

        // 作者
        [JsonProperty(PropertyName = "author")]
        public string Author { set; get; }

        // 类别
        [JsonProperty(PropertyName = "cate")]
        public string Cate { set; get; }

        // 发布日期
        [JsonProperty(PropertyName = "srcdate")]
        public string SrcDate { set; get; }

        // 推送日期
        [JsonProperty(PropertyName = "reldate")]
        public string RelDate { set; get; }

        // 热度分
        [JsonProperty(PropertyName = "score")]
        public float Score { set; get; }

        // 隐藏标记
        [JsonProperty(PropertyName = "deprecated")]
        public int Deprecated { set; get; }

        // ...
    }
}
