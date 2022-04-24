using Newtonsoft.Json;

namespace TimelineWallpaper.Beans {
    public class CrashApiReq {
        [JsonProperty(PropertyName = "app")]
        public string App { set; get; }

        [JsonProperty(PropertyName = "pkg")]
        public string Package { set; get; }

        [JsonProperty(PropertyName = "ver")]
        public string Version { set; get; }

        [JsonProperty(PropertyName = "os")]
        public string Os { set; get; }

        [JsonProperty(PropertyName = "osver")]
        public string OsVersion { set; get; }

        [JsonProperty(PropertyName = "device")]
        public string Device { set; get; }

        [JsonProperty(PropertyName = "deviceid")]
        public string DeviceId { set; get; }

        [JsonProperty(PropertyName = "exception")]
        public string Exception { set; get; }
    }
}
