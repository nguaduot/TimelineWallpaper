using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class LofterApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "background")]
        public List<LofterApiBg> Background { set; get; }

        // 日期
        [JsonProperty(PropertyName = "curdate")]
        public long CurDate { set; get; }
    }

    public class LofterApiBg {
        // 缩略图URL
        [JsonProperty(PropertyName = "image")]
        public string Image { set; get; }

        // 视频URL（与缩略图URL存其一）
        [JsonProperty(PropertyName = "video")]
        public string Video { set; get; }

        // 作者
        [JsonProperty(PropertyName = "author")]
        public LofterApiAuthor Author { set; get; }
    }

    public class LofterApiAuthor {
        // 作者
        [JsonProperty(PropertyName = "name")]
        public string Name { set; get; }

        // 作者链接
        [JsonProperty(PropertyName = "link")]
        public string Link { set; get; }
    }
}
