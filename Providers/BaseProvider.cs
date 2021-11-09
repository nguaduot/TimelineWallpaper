using TimelineWallpaper.Beans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using TimelineWallpaper.Utils;

namespace TimelineWallpaper.Providers {
    public class BaseProvider {
        public string Id { set; get; }

        protected readonly List<Meta> metas = new List<Meta>();

        // 当前浏览索引（超限取余，循环浏览）
        protected int indexFocus = 0;

        protected void RandomMetas() {
            List<Meta> metasNew = new List<Meta>();
            Random random = new Random();
            foreach (Meta meta in metas) {
                metasNew.Insert(random.Next(metasNew.Count + 1), meta);
            }
            metas.Clear();
            metas.AddRange(metasNew);
        }

        public virtual async Task<bool> LoadData(Ini ini) {
            await Task.Delay(1000);
            return false;
        }

        public Meta GetFocus() {
            if (metas.Count == 0) {
                return null;
            }
            int index = indexFocus % metas.Count;
            index = index >= 0 ? index : index + metas.Count;
            return metas[index];
        }

        public Meta GetNext(bool move = true) {
            ++indexFocus;
            Meta meta = GetFocus();
            if (!move) {
                --indexFocus;
            }
            return meta;
        }

        public Meta GetLast(bool move = true) {
            --indexFocus;
            Meta meta = GetFocus();
            if (!move) {
                ++indexFocus;
            }
            return meta;
        }

        public static async Task<Meta> Cache(BaseProvider provider, Meta meta) {
            if (meta == null || meta.IsCached()) {
                return meta;
            }
            // 缓存到临时文件夹（允许随时被清理）
            StorageFile cacheFile = await ApplicationData.Current.TemporaryFolder
                .CreateFileAsync(provider.Id + "-" + meta.Id + meta.Format, CreationCollisionOption.OpenIfExists);
            BasicProperties fileProperties = await cacheFile.GetBasicPropertiesAsync();
            if (fileProperties.Size > 0) { // 已缓存过
                meta.SetCacheOne(cacheFile);
                Debug.WriteLine("cached from disk: " + meta);
            } else if (meta.IsUrled()) {
                try {
                    BackgroundDownloader downloader = new BackgroundDownloader();
                    DownloadOperation operation = downloader.CreateDownload(new Uri(meta.GetUrlOne()), cacheFile);
                    DownloadOperation resOperation = await operation.StartAsync().AsTask();
                    if (resOperation.Progress.Status == BackgroundTransferStatus.Completed) {
                        meta.SetCacheOne(cacheFile);
                        Debug.WriteLine("cached from network: " + meta);
                    }
                } catch (Exception) {
                    Debug.WriteLine("cache error");
                }
            }
            // 提取图片主题色
            //if (meta.Cache != null) {
            //    fileProperties = await meta.Cache.GetBasicPropertiesAsync();
            //    if (fileProperties.Size > 0) {
            //        using (var stream = await meta.Cache.OpenAsync(FileAccessMode.Read)) {
            //            var decoder = await BitmapDecoder.CreateAsync(stream);
            //            var colorThief = new ColorThief();
            //            var dominantColor = await colorThief.GetColor(decoder);
            //            Debug.WriteLine("dominant color: " + dominantColor.Color);
            //            meta.Dominant = new SolidColorBrush(Windows.UI.Color.FromArgb(dominantColor.Color.A,
            //                dominantColor.Color.R, dominantColor.Color.G, dominantColor.Color.B));
            //        }
            //    }
            //}
            return meta;
        }

        public static async Task<bool> Download(BaseProvider provider, Meta meta) {
            if (meta == null || !meta.IsCached()) {
                return false;
            }

            try {
                string appName = new ResourceLoader().GetString("AppNameShort");
                StorageFolder folder = await KnownFolders.PicturesLibrary
                    .CreateFolderAsync(appName, CreationCollisionOption.OpenIfExists);
                string title = meta.GetTitleOrCaption();
                title = !string.IsNullOrEmpty(title) ? title : (!string.IsNullOrEmpty(meta.Caption) ? meta.Caption : "");
                string name = string.Format("{0}_{1}_{2}_{3}{4}", appName, provider.Id, meta.Date?.ToString("yyMMdd"), title, meta.Format);
                name = FileUtil.MakeValidFileName(name, "");
                _ = await meta.GetCacheOne().CopyAsync(folder, name, NameCollisionOption.ReplaceExisting);
                return true;
            } catch (Exception) {
                Debug.WriteLine("download error");
            }

            return false;
        }
    }
}
