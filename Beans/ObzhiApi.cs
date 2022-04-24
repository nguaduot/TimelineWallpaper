using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class ObzhiApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "data")]
        public List<ObzhiApiData> Data { set; get; }
    }

    public class ObzhiApiData {
        // 类别ID
        [JsonProperty(PropertyName = "cateidalt")]
        public string CateIdAlt { set; get; }

        // 类别
        [JsonProperty(PropertyName = "catealt")]
        public string CateAlt { set; get; }

        // 标题
        [JsonProperty(PropertyName = "title")]
        public string Title { set; get; }

        // 图文故事
        [JsonProperty(PropertyName = "story")]
        public string Story { set; get; }

        // 作者
        [JsonProperty(PropertyName = "author")]
        public string Author { set; get; }

        // 图片ID
        [JsonProperty(PropertyName = "imgid")]
        public int ImgId { set; get; }

        // 图片URL
        [JsonProperty(PropertyName = "imgurl")]
        public string ImgUrl { set; get; }

        // 缩略图URL
        [JsonProperty(PropertyName = "thumburl")]
        public string ThumbUrl { set; get; }

        // 发布日期
        [JsonProperty(PropertyName = "reldate")]
        public string RelDate { set; get; }

        // 热度分
        [JsonProperty(PropertyName = "score")]
        public float Score { set; get; }

        // R18内容
        [JsonProperty(PropertyName = "r18")]
        public int R18 { set; get; }

        // ...
    }
}
