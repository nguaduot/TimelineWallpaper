using Newtonsoft.Json;

namespace TimelineWallpaper.Beans {
    public class DmoeApiItem {
        // 图片URL
        [JsonProperty(PropertyName = "imgurl")]
        public string ImgUrl { set; get; }

        // ...
    }
}
