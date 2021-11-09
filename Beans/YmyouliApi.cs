using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class YmyouliApi {
        [JsonProperty(PropertyName = "moduleInfo")]
        public YmyouliApiModule ModuleInfo { set; get; }
    }

    public class YmyouliApiModule {
        // 栏目名
        [JsonProperty(PropertyName = "name")]
        public string Name { set; get; }

        // 图片ID表
        [JsonProperty(PropertyName = "prop5")]
        public List<YmyouliApiProp5> Props { set; get; }

        // 栏目创建日期
        [JsonProperty(PropertyName = "createTime")]
        public long CreateTime { set; get; }

        // ...
    }

    public class YmyouliApiProp5 {
        [JsonProperty(PropertyName = "id")]
        public string Id { set; get; }
    }

    public class YmyouliRequest {
        [JsonProperty(PropertyName = "cmd")]
        public string Cmd { set; get; } = "getWafNotCk_getAjaxPageModuleInfo";

        [JsonProperty(PropertyName = "href")]
        public string Href { set; get; }

        [JsonProperty(PropertyName = "_colId")]
        public string Col { set; get; }

        [JsonProperty(PropertyName = "moduleId")]
        public string Module { set; get; }
    }
}
