using System;
using System.Drawing;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace TimelineWallpaper.Beans {
    public class Meta {
        // ID（非空）
        public string Id { set; get; }

        // 原图URL
        public string Uhd { set; get; }

        // 缩略图URL
        public string Thumb { set; get; }

        // 标题
        public string Title { set; get; }

        // 副标题
        public string Caption { set; get; }

        // 类别
        public string Cate { set; get; }

        // 位置
        public string Location { set; get; }

        // 描述/图文故事
        public string Story { set; get; }

        // 版权所有者/作者
        public string Copyright { set; get; }

        // 展示日期（非空）
        public DateTime? Date { set; get; }

        // 文件格式
        public string Format { set; get; } = ".jpg";

        // 原图/视频尺寸
        public Size Dimen { set; get; }

        // 原图本地缓存文件
        public StorageFile CacheUhd { set; get; }

        public DownloadOperation Do { set; get; }

        // 主题色笔刷
        //public SolidColorBrush Dominant { set; get; }

        // 人像位置
        public float FaceOffset { set; get; } = 0.5f;

        public double SortFactor { set; get; }

        public bool IsValid() {
            return !string.IsNullOrEmpty(Id) && Date != null && Uhd != null;
        }

        public string GetTitleOrCaption() {
            return Title ?? Caption;
        }
    }
}
