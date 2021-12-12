using System;
using System.Drawing;
using Windows.Storage;

namespace TimelineWallpaper.Beans {
    public class Meta {
        // ID（非空）
        public string Id { set; get; }

        // 原图URL（原图、视频、音频暂支持存其一）
        public string Uhd { set; get; }

        // 视频URL（原图、视频、音频暂支持存其一）
        public string Video { set; get; }

        // 音频URL（原图、视频、音频暂支持存其一）
        public string Audio { set; get; }

        // 缩略图URL
        public string Thumb { set; get; }

        // 标题
        public string Title { set; get; }

        // 副标题
        public string Caption { set; get; }

        // 位置
        public string Location { set; get; }

        // 描述/图文故事
        public string Story { set; get; }

        // 版权所有者/作者
        public string Copyright { set; get; }

        // 展示日期（非空）
        public DateTime? Date { set; get; }

        // 文件格式
        public string Format { set; get; }

        // 原图/视频尺寸
        public Size Dimen { set; get; }

        // 原图本地缓存文件
        public StorageFile CacheUhd { set; get; }

        // 视频本地缓存文件
        public StorageFile CacheVideo { set; get; }

        // 音频本地缓存文件
        public StorageFile CacheAudio { set; get; }

        // 主题色笔刷
        //public SolidColorBrush Dominant { set; get; }

        public bool IsValid() {
            return !string.IsNullOrEmpty(Id) && Date != null;
        }

        public string GetTitleOrCaption() {
            return Title ?? Caption;
        }

        public string GetUrlOne() {
            return Uhd ?? Video ?? Audio;
        }

        public bool IsUrled() {
            return GetUrlOne() != null;
        }

        public void SetCacheOne(StorageFile file) {
            if (Uhd != null) {
                CacheUhd = file;
            } else if (Video != null) {
                CacheVideo = file;
            } else if (Audio != null) {
                CacheAudio = file;
            }
        }

        public StorageFile GetCacheOne() {
            return CacheUhd ?? CacheVideo ?? CacheAudio;
        }

        public bool IsCached() {
            return GetCacheOne() != null;
        }
    }
}
