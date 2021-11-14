using Newtonsoft.Json;

namespace TimelineWallpaper.Beans {
    public class MtyApiItem {
        // 图片URL
        [JsonProperty(PropertyName = "imgurl")]
        public string ImgUrl { set; get; }

        // ...
    }
}
