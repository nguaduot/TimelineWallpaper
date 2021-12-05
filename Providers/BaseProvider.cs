using TimelineWallpaper.Beans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using TimelineWallpaper.Utils;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Media.Editing;
using System.Drawing;

namespace TimelineWallpaper.Providers {
    public class BaseProvider {
        public string Id { set; get; }

        protected readonly List<Meta> metas = new List<Meta>();

        // 当前浏览索引
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

        public Meta GetFocus() => metas.Count > 0 ? metas[indexFocus] : null;

        public Meta GetNext(bool move = true) {
            int indexOld = indexFocus;
            indexFocus = indexOld >= metas.Count - 1 ? 0 : indexOld + 1;
            Meta meta = GetFocus();
            indexFocus = move ? indexFocus : indexOld;
            return meta;
        }

        public Meta GetLast(bool move = true) {
            int indexOld = indexFocus;
            indexFocus = indexOld == 0 ? metas.Count - 1 : indexOld - 1;
            Meta meta = GetFocus();
            indexFocus = move ? indexFocus : indexOld;
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
            if (meta.CacheUhd != null) {
                using(IRandomAccessStream stream = await meta.CacheUhd.OpenAsync(FileAccessMode.Read)) {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    meta.Dimen = new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                }
            }
            if (meta.CacheVideo != null) {
                MediaClip mediaClip = await MediaClip.CreateFromFileAsync(meta.CacheVideo);
                MediaComposition mediaComposition = new MediaComposition();
                mediaComposition.Clips.Add(mediaClip);
                var stream = await mediaComposition.GetThumbnailAsync(TimeSpan.FromMilliseconds(5000), 0, 0,
                    VideoFramePrecision.NearestFrame);
                var decoder = await BitmapDecoder.CreateAsync(stream);
                meta.Dimen = new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            }
            return meta;
        }

        public static async Task<StorageFile> Download(Meta meta, string appName, string provider) {
            if (meta == null || !meta.IsCached()) {
                return null;
            }

            try {
                StorageFolder folder = await KnownFolders.PicturesLibrary
                    .CreateFolderAsync(appName, CreationCollisionOption.OpenIfExists);
                string name = string.Format("{0}_{1}_{2}{3}", appName, provider, meta.Id, meta.Format);
                name = FileUtil.MakeValidFileName(name, "");
                return await meta.GetCacheOne().CopyAsync(folder, name, NameCollisionOption.ReplaceExisting);
            } catch (Exception) {
                Debug.WriteLine("download error");
            }
            return null;
        }
    }
}
