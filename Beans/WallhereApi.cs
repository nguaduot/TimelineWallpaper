using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimelineWallpaper.Beans {
    public class WallhereApi {
        // 图片信息数组
        [JsonProperty(PropertyName = "data")]
        public List<WallhereApiData> Data { set; get; }
    }

    public class WallhereApiData {
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

        // 图片ID
        [JsonProperty(PropertyName = "imgid")]
        public int ImgId { set; get; }

        // 图片URL
        [JsonProperty(PropertyName = "imgurl")]
        public string ImgUrl { set; get; }

        // 缩略图URL
        [JsonProperty(PropertyName = "thumburl")]
        public string ThumbUrl { set; get; }

        // 作者
        [JsonProperty(PropertyName = "author")]
        public string Author { set; get; }

        // TAG
        [JsonProperty(PropertyName = "tag")]
        public string Tag { set; get; }

        // 热度分
        [JsonProperty(PropertyName = "score")]
        public float Score { set; get; }

        // R18内容
        [JsonProperty(PropertyName = "r18")]
        public int R18 { set; get; }

        // ...
    }
}
