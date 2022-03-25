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
using System.Text;
using System.IO;
using System.Linq;
using Windows.Media.FaceAnalysis;

namespace TimelineWallpaper.Providers {
    public class BaseProvider {
        public string Id { set; get; }

        // 索引顺序为「回顾」顺序
        // 注：并非都按时间降序排列，因图源配置而异
        protected readonly List<Meta> metas = new List<Meta>();

        // 当前浏览索引
        protected int indexFocus = 0;

        protected Dictionary<string, int> dicHistory = new Dictionary<string, int>();

        protected void AppendMetas(List<Meta> metasAdd) {
            List<string> list = metas.Select(t => t.Id).ToList();
            foreach (Meta meta in metasAdd) {
                if (!list.Contains(meta.Id) && meta.IsValid()) {
                    metas.Add(meta);
                }
            }
        }

        protected void SortMetas(List<Meta> metasAdd) {
            AppendMetas(metasAdd);
            string idFocus = GetFocus()?.Id;
            // 按日期降序排列
            metas.Sort((m1, m2) => {
                if (m1.Date.Value > m2.Date.Value) {
                    return -1;
                }
                if (m1.Date.Value < m2.Date.Value) {
                    return 1;
                }
                return m1.Id.CompareTo(m2.Id);
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

        protected void RandomMetas(List<Meta> metasAdd) {
            //List<Meta> metasNew = new List<Meta>();
            //Random random = new Random();
            //foreach (Meta meta in metas) {
            //    metasNew.Insert(random.Next(metasNew.Count + 1), meta);
            //}
            //metas.Clear();
            //metas.AddRange(metasNew);
            Random random = new Random();
            foreach (Meta meta in metasAdd) {
                dicHistory.TryGetValue(meta.Id, out int times);
                meta.SortFactor = random.NextDouble() + (times / 10.0 > 0.9 ? 0.9 : times / 10.0);
            }
            // 升序排列，已阅图降低出现在前排的概率
            metasAdd.Sort((m1, m2) => m1.SortFactor.CompareTo(m2.SortFactor));
            AppendMetas(metasAdd);
        }

        public virtual async Task<bool> LoadData(BaseIni ini, DateTime? date = null) {
            await Task.Run(() => {
                dicHistory = GetHistory(Id);
            });
            return false;
        }

        public Meta GetFocus() {
            if (metas.Count > 0) {
                if (dicHistory.ContainsKey(metas[indexFocus].Id)) {
                    dicHistory[metas[indexFocus].Id] += 1;
                } else {
                    dicHistory[metas[indexFocus].Id] = 1;
                }
                SaveHistory(Id, dicHistory);
                return metas[indexFocus];
            }
            return null;
        }

        public Meta Yesterday() {
            indexFocus = indexFocus < metas.Count - 1 ? indexFocus + 1 : metas.Count - 1;
            if (metas.Count > 0) {
                if (dicHistory.ContainsKey(metas[indexFocus].Id)) {
                    dicHistory[metas[indexFocus].Id] += 1;
                } else {
                    dicHistory[metas[indexFocus].Id] = 1;
                }
                SaveHistory(Id, dicHistory);
                return metas[indexFocus];
            }
            return null;
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
            // 获取图片尺寸&检测人像位置
            if (meta.CacheUhd != null && FaceDetector.IsSupported) {
                using (var stream = await meta.CacheUhd.OpenAsync(FileAccessMode.Read)) {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    // 获取图片尺寸
                    meta.Dimen = new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                    // 检测人像位置
                    try {
                        // TODO: 该行会随机抛出异常 System.Exception: 图像无法识别
                        SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat,
                            BitmapAlphaMode.Premultiplied, new BitmapTransform(),
                            ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
                        if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Gray8) {
                            bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Gray8);
                        }
                        FaceDetector detector = await FaceDetector.CreateAsync();
                        IList<DetectedFace> faces = await detector.DetectFacesAsync(bitmap);
                        float offset = -1;
                        foreach (DetectedFace face in faces) {
                            offset = Math.Max(offset, (face.FaceBox.X + face.FaceBox.Width / 2.0f) / bitmap.PixelWidth);
                        }
                        meta.FaceOffset = offset >= 0 ? offset : 0.5f;
                        bitmap.Dispose();
                    } catch (Exception ex) {
                        Debug.WriteLine(ex);
                    }
                }
            }
            // 获取视频尺寸
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

        public static Dictionary<string, int> GetHistory(string provider) {
            string file = Path.Combine(ApplicationData.Current.LocalFolder.Path, provider + ".json");
            if (!File.Exists(file)) {
                return new Dictionary<string, int>();
            }
            string content = File.ReadAllText(file, UTF8Encoding.UTF8);
            return JsonConvert.DeserializeObject<Dictionary<string, int>>(content);
        }

        public static void SaveHistory(string provider, Dictionary<string, int> dic) {
            string file = Path.Combine(ApplicationData.Current.LocalFolder.Path, provider + ".json");
            File.WriteAllText(file, JsonConvert.SerializeObject(dic), UTF8Encoding.UTF8);
        }
    }
}
