using Newtonsoft.Json;

namespace TimelineWallpaper.Beans {
    public class StatsApiReq {
        // "?app={0}&pkg={1}&ver={2}&api={3}&status={4}&os={5}&osver={6}&device={7}&deviceid={8}"
        [JsonProperty(PropertyName = "app")]
        public string App { set; get; }

        [JsonProperty(PropertyName = "pkg")]
        public string Package { set; get; }

        [JsonProperty(PropertyName = "ver")]
        public string Version { set; get; }

        [JsonProperty(PropertyName = "api")]
        public string Api { set; get; }

        [JsonProperty(PropertyName = "status")]
        public int Status { set; get; }

        [JsonProperty(PropertyName = "os")]
        public string Os { set; get; }

        [JsonProperty(PropertyName = "osver")]
        public string OsVersion { set; get; }

        [JsonProperty(PropertyName = "device")]
        public string Device { set; get; }

        [JsonProperty(PropertyName = "deviceid")]
        public string DeviceId { set; get; }
    }
}
