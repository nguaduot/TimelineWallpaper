using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class InfinityApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "data")]
        public List<InfinityApiData> Data { set; get; }
    }

    public class InfinityApiData {
        // ID
        [JsonProperty(PropertyName = "_id")]
        public string Id { set; get; }

        [JsonProperty(PropertyName = "src")]
        public InfinityApiSrc Src { set; get; }

        // 标签
        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { set; get; }

        // 壁纸源
        [JsonProperty(PropertyName = "source")]
        public string Source { set; get; }

        // ...
    }

    public class InfinityApiSrc {
        // 原图URL
        [JsonProperty(PropertyName = "rawSrc")]
        public string RawSrc { set; get; }

        //缩略图URL
        [JsonProperty(PropertyName = "smallSrc")]
        public string SmallSrc { set; get; }
    }
}
