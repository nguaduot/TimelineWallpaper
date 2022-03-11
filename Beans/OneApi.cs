using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class OneApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "data")]
        public List<OneApiData> Data { set; get; }
    }

    public class OneApiData {
        // ID
        [JsonProperty(PropertyName = "id")]
        public string Id { set; get; }

        // 图片URL
        [JsonProperty(PropertyName = "img_url")]
        public string ImgUrl { set; get; }

        // 标题
        [JsonProperty(PropertyName = "title")]
        public string Title { set; get; }

        // 图文故事
        [JsonProperty(PropertyName = "content")]
        public string Content { set; get; }

        // 摄影作者
        [JsonProperty(PropertyName = "picture_author")]
        public string PictureAuthor { set; get; }

        // 文字作者
        [JsonProperty(PropertyName = "text_authors")]
        public string TextAuthors { set; get; }

        // 日期
        [JsonProperty(PropertyName = "date")]
        public string Date { set; get; }

        // ...
    }
}
