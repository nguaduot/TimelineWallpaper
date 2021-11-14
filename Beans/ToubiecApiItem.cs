using Newtonsoft.Json;

namespace TimelineWallpaper.Beans {
    public class ToubiecApiItem {
        // ID
        [JsonProperty(PropertyName = "id")]
        public int Id { set; get; }

        // 图片URL
        [JsonProperty(PropertyName = "imgurl")]
        public string ImgUrl { set; get; }

        // ...
    }
}
