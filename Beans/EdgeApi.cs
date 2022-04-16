using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class EdgeApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "configs")]
        public EdgeApiConfig Config { set; get; }
    }

    public class EdgeApiConfig {
        [JsonProperty(PropertyName = "BackgroundImageWC/default")]
        public EdgeApiBgImg BgImg { set; get; }
    }

    public class EdgeApiBgImg {
        [JsonProperty(PropertyName = "properties")]
        public EdgeApiProperty Property { set; get; }
    }

    public class EdgeApiProperty {
        [JsonProperty(PropertyName = "video")]
        public EdgeApiVideo Video { set; get; }
    }

    public class EdgeApiVideo {
        [JsonProperty(PropertyName = "data")]
        public List<EdgeApiData> Data { set; get; }
    }

    public class EdgeApiData {
        // 版权信息
        [JsonProperty(PropertyName = "attribution")]
        public string Attribution { set; get; }

        // 视频
        [JsonProperty(PropertyName = "video")]
        public EdgeApiVideo2 Video { set; get; }

        // 缩略图
        [JsonProperty(PropertyName = "firstFrame")]
        public EdgeApiFrame Frame { set; get; }

        // ...
    }

    public class EdgeApiVideo2 {
        [JsonProperty(PropertyName = "v1080")]
        public string V1080 { set; get; }

        [JsonProperty(PropertyName = "v1440")]
        public string V1440 { set; get; }

        [JsonProperty(PropertyName = "v2160")]
        public string V2160 { set; get; }
    }

    public class EdgeApiFrame {
        [JsonProperty(PropertyName = "i1080")]
        public string I1080 { set; get; }

        [JsonProperty(PropertyName = "i1440")]
        public string I1440 { set; get; }

        [JsonProperty(PropertyName = "i2160")]
        public string I2160 { set; get; }
    }
}
