using Newtonsoft.Json;

namespace TimelineWallpaper.Beans {
    public class RankApiReq {
        [JsonProperty(PropertyName = "provider")]
        public string Provider { set; get; }

        [JsonProperty(PropertyName = "imgid")]
        public string ImgId { set; get; }

        [JsonProperty(PropertyName = "imgurl")]
        public string ImgUrl { set; get; }

        [JsonProperty(PropertyName = "action")]
        public string Action { set; get; }

        [JsonProperty(PropertyName = "deviceid")]
        public string DeviceId { set; get; }

        [JsonProperty(PropertyName = "region")]
        public string Region { set; get; }

        [JsonProperty(PropertyName = "undo")]
        public bool Undo { set; get; }
    }
}
