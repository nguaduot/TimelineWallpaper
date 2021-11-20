using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class MxgApi {
        // 图片信息
        [JsonProperty(PropertyName = "data")]
        public MxgApiData Data { set; get; }
    }

    public class MxgMvApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "data")]
        public List<MxgApiData> Data { set; get; }
    }

    public class MxgApiData {
        // 图片URL
        [JsonProperty(PropertyName = "imgurl")]
        public string ImgUrl { set; get; }
    }
}
