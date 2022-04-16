using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class YmyouliApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "data")]
        public List<YmyouliApiData> Data { set; get; }
    }

    public class YmyouliApiData {
        // 排序编号
        [JsonProperty(PropertyName = "no")]
        public int No { set; get; }

        // 类别内排序序号
        [JsonProperty(PropertyName = "catealtno")]
        public int CateAltNo { set; get; }

        // 类别ID
        [JsonProperty(PropertyName = "cateidalt")]
        public string CateIdAlt { set; get; }

        // 类别
        [JsonProperty(PropertyName = "catealt")]
        public string CateAlt { set; get; }

        // 类别
        [JsonProperty(PropertyName = "cate")]
        public string Cate { set; get; }

        // 类别
        [JsonProperty(PropertyName = "group")]
        public string Group { set; get; }

        // 图片ID
        [JsonProperty(PropertyName = "imgid")]
        public string ImgId { set; get; }

        // 图片URL
        [JsonProperty(PropertyName = "imgurl")]
        public string ImgUrl { set; get; }

        // 缩略图URL
        [JsonProperty(PropertyName = "thumburl")]
        public string ThumbUrl { set; get; }

        // 版权所有
        [JsonProperty(PropertyName = "copyright")]
        public string Copyright { set; get; }

        // 热度分
        [JsonProperty(PropertyName = "score")]
        public float Score { set; get; }

        // 隐藏标记
        [JsonProperty(PropertyName = "deprecated")]
        public int Deprecated { set; get; }

        // ...
    }
}
