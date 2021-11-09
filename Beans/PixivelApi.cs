using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class PixivelApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "illusts")]
        public List<PixivelApiIllust> Illusts { set; get; }
    }

    public class PixivelApiIllust {
        // ID
        [JsonProperty(PropertyName = "id")]
        public string Id { set; get; }

        // 原图链接
        [JsonProperty(PropertyName = "meta_single_page")]
        public PixivelApiPage MetaSinglePage { set; get; }

        // 缩略图链接
        [JsonProperty(PropertyName = "image_urls")]
        public PixivelApiUrl ImageUrls { set; get; }

        // 图片链接
        [JsonProperty(PropertyName = "meta_pages")]
        public List<PixivelApiPage2> MetaPages { set; get; }

        // 日期
        [JsonProperty(PropertyName = "create_date")]
        public string CreateDate { set; get; }

        // 作者
        [JsonProperty(PropertyName = "user")]
        public PixivelApiUser User { set; get; }

        // 标题
        [JsonProperty(PropertyName = "title")]
        public string Title { set; get; }

        // 描述
        [JsonProperty(PropertyName = "caption")]
        public string Caption { set; get; }

        // 敏感度（1-10）
        [JsonProperty(PropertyName = "sanity_level")]
        public int SanityLevel { set; get; }

        // ...
    }

    public class PixivelApiPage {
        // 原图链接
        [JsonProperty(PropertyName = "original_image_url")]
        public string OriginalImageUrl { set; get; }
    }

    public class PixivelApiPage2 {
        // 原图链接
        [JsonProperty(PropertyName = "image_urls")]
        public PixivelApiUrl2 ImageUrls { set; get; }
    }

    public class PixivelApiUrl {
        // 大图链接
        [JsonProperty(PropertyName = "large")]
        public string Large { set; get; }

        // 中图链接
        [JsonProperty(PropertyName = "medium")]
        public string Medium { set; get; }
    }

    public class PixivelApiUrl2 {
        // 原图链接
        [JsonProperty(PropertyName = "original")]
        public string Original { set; get; }

        // 大图链接
        [JsonProperty(PropertyName = "large")]
        public string Large { set; get; }

        // 中图链接
        [JsonProperty(PropertyName = "medium")]
        public string Medium { set; get; }
    }

    public class PixivelApiUser {
        // 作者
        [JsonProperty(PropertyName = "name")]
        public string Name { set; get; }
    }
}
