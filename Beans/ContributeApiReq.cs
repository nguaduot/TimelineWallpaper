using Newtonsoft.Json;

namespace TimelineWallpaper.Beans {
    public class ContributeApiReq {
        [JsonProperty(PropertyName = "url")]
        public string Url { set; get; }

        [JsonProperty(PropertyName = "title")]
        public string Title { set; get; }

        [JsonProperty(PropertyName = "story")]
        public string Story { set; get; }

        [JsonProperty(PropertyName = "contact")]
        public string Contact { set; get; }

        [JsonProperty(PropertyName = "appver")]
        public string AppVer { set; get; }
    }
}
