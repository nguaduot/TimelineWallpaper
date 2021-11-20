using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class CoolapkApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "data")]
        public List<CoolapkApiData> Data { set; get; }
    }

    public class CoolapkApiData {
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

        // 描述
        [JsonProperty(PropertyName = "story")]
        public string Story { set; get; }

        // 作者
        [JsonProperty(PropertyName = "author")]
        public string Author { set; get; }

        // 发布日期
        [JsonProperty(PropertyName = "srcdate")]
        public string SrcDate { set; get; }

        // 推送日期
        [JsonProperty(PropertyName = "reldate")]
        public string RelDate { set; get; }

        // ...
    }
}
