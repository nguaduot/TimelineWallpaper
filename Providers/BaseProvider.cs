using TimelineWallpaper.Beans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using TimelineWallpaper.Utils;
using Windows.Graphics.Imaging;
using Windows.Media.Editing;
using System.Drawing;
using Newtonsoft.Json;

namespace TimelineWallpaper.Providers {
    public class BaseProvider {
        public string Id { set; get; }

        // 索引顺序为「回顾」顺序
        // 注：并非都按时间降序排列，因图源配置而异
        protected readonly List<Meta> metas = new List<Meta>();

        // 当前浏览索引
        protected int indexFocus = 0;

        protected void SortMetas() {
            string idFocus = GetFocus()?.Id;
            // 按日期降序排列
            metas.Sort((m1, m2) => {
                if (m1.Date.Value > m2.Date.Value) {
                    return -1;
                }
                if (m1.Date.Value < m2.Date.Value) {
                    return 1;
                }
                return m1.Id.CompareTo(m2);
            });
            // 恢复当前索引
            if (indexFocus > 0) {
                for (int i = 0; i < metas.Count; i++) {
                    if (metas[i].Id == idFocus) {
                        indexFocus = i;
                        break;
                    }
                }
            }
        }

        protected void RandomMetas() {
            List<Meta> metasNew = new List<Meta>();
            Random random = new Random();
            foreach (Meta meta in metas) {
                metasNew.Insert(random.Next(metasNew.Count + 1), meta);
            }
            metas.Clear();
            metas.AddRange(metasNew);
        }

        public virtual async Task<bool> LoadData(BaseIni ini, DateTime? date = null) {
            await Task.Delay(1000);
            return false;
        }

        public Meta GetFocus() => metas.Count > 0 ? metas[indexFocus] : null;

        public Meta Yesterday() {
            indexFocus = indexFocus < metas.Count - 1 ? indexFocus + 1 : metas.Count - 1;
            return metas.Count > 0 ? metas[indexFocus] : null;
        }

        public Meta GetYesterday() {
            int index = indexFocus < metas.Count - 1 ? indexFocus + 1 : metas.Count - 1;
            return metas.Count > 0 ? metas[index] : null;
        }

        public Meta Tormorrow() {
            indexFocus = indexFocus > 0 ? indexFocus - 1 : 0;
            return metas.Count > 0 ? metas[indexFocus] : null;
        }

        public Meta GetTormorrow() {
            int index = indexFocus > 0 ? indexFocus - 1 : 0;
            return metas.Count > 0 ? metas[index] : null;
        }

        public Meta Latest() {
            Meta metaFarthest = metas.Count > 0 ? metas[0] : null;
            for (int i = 0; i < metas.Count; i++) {
                if (metas[i].Date >= metaFarthest.Date) {
                    indexFocus = i;
                    metaFarthest = metas[i];
                }
            }
            return metaFarthest;
        }

        public Meta GetLatest() {
            Meta metaFarthest = metas.Count > 0 ? metas[0] : null;
            for (int i = 0; i < metas.Count; i++) {
                if (metas[i].Date >= metaFarthest.Date) {
                    metaFarthest = metas[i];
                }
            }
            return metaFarthest;
        }

        public Meta Farthest() {
            Meta metaFarthest = metas.Count > 0 ? metas[0] : null;
            for (int i = 0; i < metas.Count; i++) {
                if (metas[i].Date <= metaFarthest.Date) {
                    indexFocus = i;
                    metaFarthest = metas[i];
                }
            }
            return metaFarthest;
        }

        public Meta GetFarthest() {
            Meta metaFarthest = metas.Count > 0 ? metas[0] : null;
            for (int i = 0; i < metas.Count; i++) {
                if (metas[i].Date <= metaFarthest.Date) {
                    metaFarthest = metas[i];
                }
            }
            return metaFarthest;
        }

        public Meta Target(DateTime date) {
            if (date == null) {
                return null;
            }
            for (int i = 0; i < metas.Count; i++) {
                if (date.ToString("yyyyMMdd").Equals(metas[i].Date?.ToString("yyyyMMdd"))) {
                    indexFocus = i;
                    return metas[i];
                }
            }
            return null;
        }

        public virtual async Task<Meta> Cache(Meta meta) {
            if (meta == null || meta.IsCached()) {
                return meta;
            }
            // 缓存到临时文件夹（允许随时被清理）
            StorageFile cacheFile = await ApplicationData.Current.TemporaryFolder
                .CreateFileAsync(Id + "-" + meta.Id + meta.Format, CreationCollisionOption.OpenIfExists);
            BasicProperties fileProperties = await cacheFile.GetBasicPropertiesAsync();
            if (fileProperties.Size > 0) { // 已缓存过
                meta.SetCacheOne(cacheFile);
                Debug.WriteLine("cached from disk: " + JsonConvert.SerializeObject(meta).Trim());
            } else if (meta.IsUrled()) {
                try {
                    BackgroundDownloader downloader = new BackgroundDownloader();
                    DownloadOperation operation = downloader.CreateDownload(new Uri(meta.GetUrlOne()), cacheFile);
                    //Progress<DownloadOperation> progress = new Progress<DownloadOperation>((op) => {
                    //    if (op.Progress.TotalBytesToReceive > 0 && op.Progress.BytesReceived > 0) {
                    //        ulong value = op.Progress.BytesReceived * 100 / op.Progress.TotalBytesToReceive;
                    //        Debug.WriteLine("progress: " + value + "%");
                    //    }
                    //});
                    //DownloadOperation resOperation = await operation.StartAsync().AsTask(progress);
                    DownloadOperation resOperation = await operation.StartAsync();
                    if (resOperation.Progress.Status == BackgroundTransferStatus.Completed) {
                        meta.SetCacheOne(cacheFile);
                        Debug.WriteLine("cached from network: " + JsonConvert.SerializeObject(meta).Trim());
                    }
                } catch (Exception) {
                    Debug.WriteLine("cache error");
                }
            }
            if (meta.CacheUhd != null) {
                using(var stream = await meta.CacheUhd.OpenAsync(FileAccessMode.Read)) {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    meta.Dimen = new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                }
            }
            if (meta.CacheVideo != null) {
                try {
                    MediaClip mediaClip = await MediaClip.CreateFromFileAsync(meta.CacheVideo);
                    MediaComposition mediaComposition = new MediaComposition();
                    mediaComposition.Clips.Add(mediaClip);
                    var stream = await mediaComposition.GetThumbnailAsync(TimeSpan.FromMilliseconds(5000), 0, 0,
                        VideoFramePrecision.NearestFrame);
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    meta.Dimen = new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                } catch (Exception) {
                    Debug.WriteLine("dimen error");
                }
            }
            return meta;
        }

        public async Task<StorageFile> Download(Meta meta, string appName, string provider) {
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
