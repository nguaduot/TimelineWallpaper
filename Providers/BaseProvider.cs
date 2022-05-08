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
using System.Drawing;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Linq;
using Windows.Media.FaceAnalysis;
using System.Threading;

namespace TimelineWallpaper.Providers {
    public class BaseProvider {
        public string Id { set; get; }

        // 索引顺序为「回顾」顺序
        // 注：并非都按时间降序排列，因图源配置而异
        protected readonly List<Meta> metas = new List<Meta>();

        // 当前浏览索引
        protected int indexFocus = 0;

        protected Dictionary<string, int> dicHistory = new Dictionary<string, int>();

        private BackgroundDownloader downloader = new BackgroundDownloader();
        private Queue<DownloadOperation> activeDownloads = new Queue<DownloadOperation>();
        private CancellationTokenSource cancelToken = new CancellationTokenSource();

        private static int POOL_CACHE = 5;

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
                if (m1.SortFactor > m2.SortFactor) {
                    return -1;
                }
                if (m1.SortFactor < m2.SortFactor) {
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
            indexFocus = 0;
            return metas.Count > 0 ? metas[indexFocus] : null;
        }

        public Meta GetLatest() {
            return metas.Count > 0 ? metas[0] : null;
        }

        public Meta Farthest() {
            indexFocus = metas.Count - 1;
            return metas.Count > 0 ? metas[indexFocus] : null;
        }

        public Meta GetFarthest() {
            return metas.Count > 0 ? metas[metas.Count - 1] : null;
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
            Debug.WriteLine("Cache() " + meta?.Id);
            int index = -1;
            for (int i = 0; i < metas.Count; i++) { // 定位索引以便缓存多个
                if (meta != null && metas[i].Id.Equals(meta.Id)) {
                    index = i;
                    break;
                }
            }
            if (index < 0) {
                return null;
            }
            Debug.WriteLine("Cache() index " + index);
            IReadOnlyList<DownloadOperation> historyDownloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            Debug.WriteLine("Cache() current downloads " + historyDownloads.Count);
            for (int i = index; i < Math.Min(metas.Count, index + POOL_CACHE); i++) { // 缓存多个
                Meta m = metas[i];
                if (m.CacheUhd != null) { // 无需缓存
                    continue;
                }
                string cacheName = string.Format("{0}-{1}{2}", Id, m.Id, m.Format);
                StorageFile cacheFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(cacheName, CreationCollisionOption.OpenIfExists);
                BasicProperties fileProperties = await cacheFile.GetBasicPropertiesAsync();
                if (fileProperties.Size > 0) { // 已缓存过
                    m.CacheUhd = cacheFile;
                    Debug.WriteLine("Cache() cached from disk: " + cacheFile.Path);
                } else if (m.Uhd != null && m.Do == null) { // 开始缓存
                    Debug.WriteLine("Cache() cache from network: " + m.Uhd);
                    foreach (DownloadOperation o in historyDownloads) { // 从历史中恢复任务
                        if (m.Uhd.Equals(o.RequestedUri)) {
                            m.Do = o;
                            break;
                        }
                    }
                    if (m.Do == null) { // 新建下载任务
                        try {
                            m.Do = downloader.CreateDownload(new Uri(m.Uhd), cacheFile);
                            _ = m.Do.StartAsync();
                        } catch (Exception e) {
                            Debug.WriteLine(e);
                        }
                    }
                    if (activeDownloads.Count >= 5) { // 暂停缓存池之外的任务
                        DownloadOperation o = activeDownloads.Dequeue();
                        if (o.Progress.Status == BackgroundTransferStatus.Running) {
                            o.Pause();
                        }
                    }
                    activeDownloads.Enqueue(m.Do);
                }
            }
            // 等待当前任务下载完成
            if (meta.CacheUhd == null && meta.Do != null) {
                Debug.WriteLine("Cache() wait for cache: " + meta.Do.Guid);
                try {
                    if (meta.Do.Progress.Status == BackgroundTransferStatus.PausedByApplication) {
                        meta.Do.Resume();
                    }
                    _ = await meta.Do.AttachAsync();
                    if (meta.Do.Progress.Status == BackgroundTransferStatus.Completed) {
                        meta.CacheUhd = meta.Do.ResultFile as StorageFile;
                    }
                } catch (Exception e) {
                    // 未找到(404)。 (Exception from HRESULT: 0x80190194)
                    Debug.WriteLine(e);
                }
            }
            // 获取图片尺寸&检测人像位置
            if (meta.CacheUhd != null && FaceDetector.IsSupported) {
                try {
                    using (var stream = await meta.CacheUhd.OpenAsync(FileAccessMode.Read)) {
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        // 获取图片尺寸
                        meta.Dimen = new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                        // 检测人像位置
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
                    }
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                }
            }
            return meta;
        }

        public async Task<StorageFile> Download(Meta meta, string appName, string provider) {
            if (meta?.CacheUhd == null) {
                return null;
            }

            try {
                StorageFolder folder = await KnownFolders.PicturesLibrary
                    .CreateFolderAsync(appName, CreationCollisionOption.OpenIfExists);
                string name = string.Format("{0}_{1}_{2}{3}", appName, provider, meta.Id, meta.Format);
                name = FileUtil.MakeValidFileName(name, "");
                return await meta.CacheUhd.CopyAsync(folder, name, NameCollisionOption.ReplaceExisting);
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
            try {
                string file = Path.Combine(ApplicationData.Current.LocalFolder.Path, provider + ".json");
                File.WriteAllText(file, JsonConvert.SerializeObject(dic), UTF8Encoding.UTF8);
            } catch (Exception) {
                Debug.WriteLine("save history error");
            }
        }
    }
}
