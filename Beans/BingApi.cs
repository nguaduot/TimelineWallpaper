using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class BingApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "images")]
        public List<BingApiImg> Images { set; get; }
    }

    public class BingApiImg {
        // ID
        [JsonProperty(PropertyName = "hsh")]
        public string Hsh { set; get; }

        // 根链接，如：/az/hprichbg/rb/Shanghai_ZH-CN10665657954
        // 前接 http://s.cn.bing.net 或 https://cn.bing.com，后接 _1920x1080.jpg 等
        [JsonProperty(PropertyName = "urlbase")]
        public string UrlBase { set; get; }

        // 日期
        [JsonProperty(PropertyName = "enddate")]
        public string EndDate { set; get; }

        // 说明+版权
        [JsonProperty(PropertyName = "copyright")]
        public string Copyright { set; get; }

        // 标题
        [JsonProperty(PropertyName = "title")]
        public string Title { set; get; }

        // 描述
        [JsonProperty(PropertyName = "desc")]
        public string Desc { set; get; }

        // ...
    }
}
