using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class QingbzApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "data")]
        public List<QingbzApiData> Data { set; get; }
    }

    public class QingbzApiData {
        // 类别内排序序号
        [JsonProperty(PropertyName = "catealtno")]
        public int CateAltNo { set; get; }

        // 类别ID
        [JsonProperty(PropertyName = "cateidalt")]
        public string CateIdAlt { set; get; }

        // 类别
        [JsonProperty(PropertyName = "catealt")]
        public string CateAlt { set; get; }

        // 图片ID
        [JsonProperty(PropertyName = "imgid")]
        public int ImgId { set; get; }

        // 图片URL
        [JsonProperty(PropertyName = "imgurl")]
        public string ImgUrl { set; get; }

        // 缩略图URL
        [JsonProperty(PropertyName = "thumburl")]
        public string ThumbUrl { set; get; }

        // 发布日期
        [JsonProperty(PropertyName = "reldate")]
        public string RelDate { set; get; }

        // 热度分
        [JsonProperty(PropertyName = "score")]
        public float Score { set; get; }

        // 隐藏标记
        [JsonProperty(PropertyName = "deprecated")]
        public int Deprecated { set; get; }

        // ...
    }
}
