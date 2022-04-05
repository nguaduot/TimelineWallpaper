using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Threading.Tasks;
using TimelineWallpaper.Beans;
using TimelineWallpaper.Providers;
using TimelineWallpaper.Services;
using TimelineWallpaper.Utils;
using TimelineWallpaperService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System;
using Windows.System.UserProfile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace TimelineWallpaper {
    public delegate void BtnInfoLinkHandler();

    public sealed partial class MainPage : Page {
        private event BtnInfoLinkHandler InfoLink;

        private readonly ResourceLoader resLoader;
        private readonly ApplicationDataContainer localSettings;

        private Ini ini = null;
        private BaseProvider provider = null;
        private Meta meta = null;

        private ReleaseApi release = null;

        private DispatcherTimer resizeTimer = null;
        private DispatcherTimer stretchTimer = null;

        private const string BG_TASK_NAME = "PushTask";
        private const string BG_TASK_NAME_TIMER = "PushTaskTimer";
        private const int MIN_COST_OF_LOAD = 800;

        public MainPage() {
            this.InitializeComponent();

            resLoader = ResourceLoader.GetForCurrentView();
            localSettings = ApplicationData.Current.LocalSettings;
            Init();
            LoadFocusAsync();
            CheckUpdateAsync();
        }

        private void Init() {
            // 启动时页面获得焦点，使快捷键一开始即可用
            this.IsTabStop = true;

            MpeUhd.MediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
            MpeUhd.MediaPlayer.IsLoopingEnabled = true;
            MpeUhd.MediaPlayer.Volume = 0;

            TextTitle.Text = resLoader.GetString("AppDesc");
            
            // 前者会在应用启动时触发多次，后者仅一次
            //this.SizeChanged += Current_SizeChanged;
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        private async void LoadFocusAsync() {
            _ = await InitProvider();
            if (!await provider.LoadData(ini.GetIni())) {
                Debug.WriteLine("failed to load data");
                ShowText(null);
                ApiService.Stats(ini, false);
                return;
            }

            meta = provider.GetFocus();
            Debug.WriteLine("focus: " + JsonConvert.SerializeObject(meta).Trim());
            ShowText(meta);
            Meta metaCache = await provider.Cache(meta);
            if (metaCache != null && metaCache.IsValid() && metaCache.Id == meta?.Id) {
                ShowImg(meta);
                ShowTips();
                ChecReviewAsync();
                ApiService.Stats(ini, true);
            }

            // 预加载
            PreLoadYesterdayAsync();
        }

        private async void LoadYesterdayAsync() {
            long cost = DateTime.Now.Ticks;
            if (!await provider.LoadData(ini.GetIni())) {
                Debug.WriteLine("failed to load data");
                if ((cost = DateTime.Now.Ticks - cost) / 10000 < MIN_COST_OF_LOAD) {
                    await Task.Delay(MIN_COST_OF_LOAD - (int)(cost / 10000));
                }
                ShowText(null);
                return;
            }

            meta = provider.Yesterday();
            Debug.WriteLine("yesterday: " + JsonConvert.SerializeObject(meta).Trim());
            ShowText(meta);
            Meta metaCache = await provider.Cache(meta);
            if ((cost = DateTime.Now.Ticks - cost) / 10000 < MIN_COST_OF_LOAD) {
                await Task.Delay(MIN_COST_OF_LOAD - (int)(cost / 10000));
            }
            if (metaCache != null && metaCache.IsValid() && metaCache.Id == meta?.Id) {
                ShowImg(meta);
                ChecReviewAsync();
            }

            // 预加载
            PreLoadYesterdayAsync();
        }

        private async void LoadTormorrowAsync() {
            long cost = DateTime.Now.Ticks;
            if (!await provider.LoadData(ini.GetIni())) {
                Debug.WriteLine("failed to load data");
                if ((cost = DateTime.Now.Ticks - cost) / 10000 < MIN_COST_OF_LOAD) {
                    await Task.Delay(MIN_COST_OF_LOAD - (int)(cost / 10000));
                }
                ShowText(null);
                return;
            }

            meta = provider.Tormorrow();
            Debug.WriteLine("tormorrow: " + JsonConvert.SerializeObject(meta).Trim());
            ShowText(meta);
            Meta metaCache = await provider.Cache(meta);
            if ((cost = DateTime.Now.Ticks - cost) / 10000 < MIN_COST_OF_LOAD) {
                await Task.Delay(MIN_COST_OF_LOAD - (int)(cost / 10000));
            }
            if (metaCache != null && metaCache.IsValid() && metaCache.Id == meta?.Id) {
                ShowImg(meta);
            }

            // 预加载
            PreLoadYesterdayAsync();
        }

        private async void LoadTargetAsync(DateTime date) {
            long cost = DateTime.Now.Ticks;
            if (!await provider.LoadData(ini.GetIni(), date)) {
                Debug.WriteLine("failed to load data");
                if ((cost = DateTime.Now.Ticks - cost) / 10000 < MIN_COST_OF_LOAD) {
                    await Task.Delay(MIN_COST_OF_LOAD - (int)(cost / 10000));
                }
                ShowText(null);
                return;
            }

            meta = provider.Target(date);
            Debug.WriteLine("meta: " + JsonConvert.SerializeObject(meta).Trim());
            ShowText(meta);
            Meta metaCache = await provider.Cache(meta);
            if ((cost = DateTime.Now.Ticks - cost) / 10000 < MIN_COST_OF_LOAD) {
                await Task.Delay(MIN_COST_OF_LOAD - (int)(cost / 10000));
            }
            if (metaCache != null && metaCache.IsValid() && metaCache.Id == meta?.Id) {
                ShowImg(meta);
            }

            // 预加载
            PreLoadYesterdayAsync();
        }

        private async void PreLoadYesterdayAsync() {
            Debug.WriteLine("PreLoadYesterdayAsync");
            if (await provider.LoadData(ini.GetIni())) {
                _ = provider.Cache(provider.GetYesterday());
            }
        }

        private async Task<bool> InitProvider() {
            if (ini == null) {
                ini = await IniUtil.GetIniAsync();
            }
            if (provider != null && provider.Id.Equals(ini.Provider)) {
                return true;
            }
            provider = ini.GenerateProvider();

            //MenuPushDesktop.Label = string.Format(resLoader.GetString("PushDesktop"), resLoader.GetString("Provider_" + provider.Id));
            //MenuPushLock.Label = string.Format(resLoader.GetString("PushLock"), resLoader.GetString("Provider_" + provider.Id));
            MenuCurDesktop.Label = string.Format(resLoader.GetString("CurDesktop"), resLoader.GetString("Provider_" + ini.DesktopProvider));
            MenuCurLock.Label = string.Format(resLoader.GetString("CurLock"), resLoader.GetString("Provider_" + ini.LockProvider));
            if (string.IsNullOrEmpty(ini.DesktopProvider)) {
                MenuPushDesktopIcon.Visibility = Visibility.Collapsed;
                MenuCurDesktopIcon.Visibility = Visibility.Collapsed;
                MenuCurDesktop.Visibility = Visibility.Collapsed;
            } else if (ini.DesktopProvider.Equals(ini.Provider)) {
                MenuPushDesktopIcon.Visibility = Visibility.Visible;
                MenuCurDesktopIcon.Visibility = Visibility.Collapsed;
                MenuCurDesktop.Visibility = Visibility.Collapsed;
            } else {
                MenuPushDesktopIcon.Visibility = Visibility.Collapsed;
                MenuCurDesktopIcon.Visibility = Visibility.Visible;
                MenuCurDesktop.Visibility = Visibility.Visible;
            }
            if (string.IsNullOrEmpty(ini.LockProvider)) {
                MenuPushLockIcon.Visibility = Visibility.Collapsed;
                MenuCurLock.Visibility = Visibility.Collapsed;
            } else if (ini.LockProvider.Equals(ini.Provider)) {
                MenuPushLockIcon.Visibility = Visibility.Visible;
                MenuCurLock.Visibility = Visibility.Collapsed;
            } else {
                MenuPushLockIcon.Visibility = Visibility.Collapsed;
                MenuCurLockIcon.Visibility = Visibility.Visible;
                MenuCurLock.Visibility = Visibility.Visible;
            }
            if (string.IsNullOrEmpty(ini.DesktopProvider) && string.IsNullOrEmpty(ini.LockProvider)) {
                UnregService();
            } else {
                _ = RegService();
                if (ini.DesktopProvider.Equals(ini.Provider) || ini.LockProvider.Equals(ini.Provider)) {
                    RunServiceNow(); // 用户浏览图源与推送图源一致，立即推送一次
                }
            }

            RadioMenuFlyoutItem item = SubmenuProvider.Items.Cast<RadioMenuFlyoutItem>().FirstOrDefault(c => ini.Provider.Equals(c?.Tag?.ToString()));
            if (item != null) {
                item.IsChecked = true;
            }
            
            return true;
        }

        private void ShowText(Meta meta) {
            if (meta == null) {
                StatusError();
                return;
            }

            // 标题按图片标题 > 图片副标题 > APP名称优先级显示，不会为空
            if (!string.IsNullOrEmpty(meta.Title)) {
                TextTitle.Text = meta.Title;
                TextDetailCaption.Text = meta.Caption ?? "";
            } else {
                TextTitle.Text = !string.IsNullOrEmpty(meta.Caption) ? meta.Caption
                    : resLoader.GetString("Slogan_" + provider.Id);
                TextDetailCaption.Text = "";
            }
            TextDetailCaption.Visibility = TextDetailCaption.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            // 位置
            TextDetailLocation.Text = meta.Location ?? "";
            TextDetailLocation.Visibility = TextDetailLocation.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            // 图文故事
            TextDetailStory.Text = meta.Story ?? "";
            TextDetailStory.Visibility = TextDetailStory.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            // 版权所有者
            TextDetailCopyright.Text = meta.Copyright ?? "";
            TextDetailCopyright.Visibility = TextDetailCopyright.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            // 日期（保持可见）
            TextDetailDate.Text = meta.Date?.ToLongDateString();
            TextDetailDate.Visibility = Visibility.Visible;
            // 文件属性（保持可见）
            TextDetailProperties.Text = "";
            TextDetailProperties.Visibility = Visibility.Visible;
        }

        private void ShowImg(Meta meta) {
            if (meta == null) {
                StatusError();
                return;
            }
            MpeUhd.Source = meta.CacheVideo != null ? MediaSource.CreateFromStorageFile(meta.CacheVideo) : null;
            if (meta.CacheUhd != null) {
                float winW = Window.Current.Content.ActualSize.X;
                float winH = Window.Current.Content.ActualSize.Y;
                BitmapImage biUhd = new BitmapImage();
                ImgUhd.Source = biUhd;
                biUhd.DecodePixelType = DecodePixelType.Logical;
                if (meta.Dimen.Width * 1.0f / meta.Dimen.Height > winW / winH) { // 图片比窗口宽，缩放至与窗口等高
                    biUhd.DecodePixelWidth = (int)Math.Round(winH * meta.Dimen.Width / meta.Dimen.Height);
                    biUhd.DecodePixelHeight = (int)Math.Round(winH);
                } else { // 图片比窗口窄，缩放至与窗口等宽
                    biUhd.DecodePixelWidth = (int)Math.Round(winW);
                    biUhd.DecodePixelHeight = (int)Math.Round(winW * meta.Dimen.Height / meta.Dimen.Width);
                }
                Debug.WriteLine("img pixel: {0}x{1}, win logical: {2}x{3}, scale logical: {4}x{5}",
                    meta.Dimen.Width, meta.Dimen.Height, winW, winH, biUhd.DecodePixelWidth, biUhd.DecodePixelHeight);
                biUhd.UriSource = new Uri(meta.CacheUhd.Path, UriKind.Absolute);
            } else {
                ImgUhd.Source = null;
            }

            string source = resLoader.GetString("Provider_" + provider.Id);
            if (meta.Cate != null) {
                source += " · " + meta.Cate;
            }
            StorageFile file = meta.CacheUhd ?? meta.CacheVideo ?? meta.CacheAudio;
            string fileSize = FileUtil.ConvertFileSize(file != null ? new FileInfo(file.Path).Length : 0);
            TextDetailProperties.Text = string.Format(resLoader.GetString("DetailSize"),
                source, meta.Dimen.Width, meta.Dimen.Height, fileSize);
            TextDetailProperties.Visibility = Visibility.Visible;
            MenuSetDesktop.IsEnabled = meta.CacheUhd != null;
            MenuSetLock.IsEnabled = meta.CacheUhd != null;
            MenuVolumnOn.Visibility = (meta.CacheVideo != null || meta.CacheAudio != null) && MpeUhd.MediaPlayer.Volume == 0
                ? Visibility.Visible : Visibility.Collapsed;
            MenuVolumnOff.Visibility = (meta.CacheVideo != null || meta.CacheAudio != null) && MpeUhd.MediaPlayer.Volume > 0
                ? Visibility.Visible : Visibility.Collapsed;
            MenuSave.IsEnabled = meta.CacheUhd != null || meta.CacheVideo != null || meta.CacheAudio != null;
            MenuDislike.IsEnabled = meta.CacheUhd != null || meta.CacheVideo != null || meta.CacheAudio != null;
            MenuFillOn.IsEnabled = meta.CacheUhd != null || meta.CacheVideo != null || meta.CacheAudio != null;
            MenuFillOff.IsEnabled = meta.CacheUhd != null || meta.CacheVideo != null || meta.CacheAudio != null;

            // 根据人脸识别优化组件放置位置
            bool faceLeft = meta.FaceOffset < 0.4;
            RelativePanel.SetAlignLeftWithPanel(ViewBarPointer, !faceLeft);
            RelativePanel.SetAlignRightWithPanel(ViewBarPointer, faceLeft);
            RelativePanel.SetAlignLeftWithPanel(Info, !faceLeft);
            RelativePanel.SetAlignRightWithPanel(Info, faceLeft);
            RelativePanel.SetAlignLeftWithPanel(AnchorGo, faceLeft);
            RelativePanel.SetAlignRightWithPanel(AnchorGo, !faceLeft);
        }

        private void ReDecodeImg() {
            if (ImgUhd.Source == null) {
                return;
            }
            BitmapImage bi = ImgUhd.Source as BitmapImage;
            if (bi.PixelHeight == 0) {
                Debug.WriteLine("ReDecodeImg(): bi.PixelWidth 0");
                return;
            }
            Debug.WriteLine("ReDecodeImg()");
            bi.DecodePixelType = DecodePixelType.Logical;
            float winW = Window.Current.Content.ActualSize.X;
            float winH = Window.Current.Content.ActualSize.Y;
            if (bi.PixelWidth * 1.0f / bi.PixelHeight > winW / winH) { // 图片比窗口宽，缩放至与窗口等高
                bi.DecodePixelWidth = (int)Math.Round(winH * bi.PixelWidth / bi.PixelHeight);
                bi.DecodePixelHeight = (int)Math.Round(winH);
            } else { // 图片比窗口窄，缩放至与窗口等宽
                bi.DecodePixelWidth = (int)Math.Round(winW);
                bi.DecodePixelHeight = (int)Math.Round(winW * bi.PixelHeight / bi.PixelWidth);
            }
            Debug.WriteLine("img pixel: {0}x{1}, win logical: {2}x{3}, scale logical: {4}x{5}",
                bi.PixelWidth, bi.PixelHeight, winW, winH, bi.DecodePixelWidth, bi.DecodePixelHeight);
        }

        private void StatusEnjoy() {
            ImgUhd.Opacity = 1;
            ImgUhd.Scale = new Vector3(1, 1, 1);
            MpeUhd.Opacity = 1;
            MpeUhd.Scale = new Vector3(1, 1, 1);
            ProgressLoading.ShowPaused = true;
            ProgressLoading.ShowError = false;
            ProgressLoading.Visibility = ViewStory.Visibility;
        }

        private void StatusLoading() {
            ImgUhd.Opacity = 0;
            ImgUhd.Scale = new Vector3(1.014f, 1.014f, 1.014f);
            MpeUhd.Opacity = 0;
            MpeUhd.Scale = new Vector3(1.014f, 1.014f, 1.014f);
            ProgressLoading.ShowPaused = false;
            ProgressLoading.ShowError = false;
            ProgressLoading.Visibility = Visibility.Visible;
            ToggleInfo(null, null);
        }

        private void StatusError() {
            ImgUhd.Opacity = 0;
            MpeUhd.Opacity = 0;
            TextTitle.Text = resLoader.GetString("AppDesc");
            TextDetailCaption.Visibility = Visibility.Collapsed;
            TextDetailLocation.Visibility = Visibility.Collapsed;
            TextDetailStory.Visibility = Visibility.Collapsed;
            TextDetailCopyright.Visibility = Visibility.Collapsed;
            TextDetailDate.Visibility = Visibility.Collapsed;
            TextDetailProperties.Visibility = Visibility.Collapsed;
            ProgressLoading.ShowError = true;
            ProgressLoading.Visibility = Visibility.Visible;

            MenuSetDesktop.IsEnabled = false;
            MenuSetLock.IsEnabled = false;
            MenuVolumnOn.Visibility = Visibility.Collapsed;
            MenuVolumnOff.Visibility = Visibility.Collapsed;
            MenuSave.IsEnabled = false;
            MenuDislike.IsEnabled = false;
            MenuFillOn.IsEnabled = false;
            MenuFillOff.IsEnabled = false;

            ToggleInfo(null, !NetworkInterface.GetIsNetworkAvailable() ? resLoader.GetString("MsgNoInternet")
                : string.Format(resLoader.GetString("MsgLostProvider"), resLoader.GetString("Provider_" + provider.Id)));
        }

        private async void ShowTips() {
            if (localSettings.Values.ContainsKey("MenuLearned")) {
                return;
            }
            await Task.Delay(3000);
            if (localSettings.Values.ContainsKey("MenuLearned")) {
                return;
            }
            TipMenu.IsOpen = true;
        }

        private async void SetWallpaperAsync(Meta meta, bool setDesktopOrLock) {
            if (meta?.CacheUhd == null) {
                return;
            }

            if (!UserProfilePersonalizationSettings.IsSupported()) {
                ToggleInfo(null, resLoader.GetString("MsgWallpaper0"));
                return;
            }
            // Your app can't set wallpapers from any folder.
            // Copy file in ApplicationData.Current.LocalFolder and set wallpaper from there.
            StorageFile fileWallpaper = await meta.CacheUhd.CopyAsync(ApplicationData.Current.LocalFolder,
                setDesktopOrLock ? "desktop" : "lock", NameCollisionOption.ReplaceExisting);
            Debug.WriteLine(fileWallpaper.Path);
            UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
            bool wallpaperSet = setDesktopOrLock
                ? await profileSettings.TrySetWallpaperImageAsync(fileWallpaper)
                : await profileSettings.TrySetLockScreenImageAsync(fileWallpaper);
            if (wallpaperSet) {
                ToggleInfo(null, resLoader.GetString(setDesktopOrLock ? "MsgSetDesktop1" : "MsgSetLock1"), InfoBarSeverity.Success);
            } else {
                ToggleInfo(null, resLoader.GetString(setDesktopOrLock ? "MsgSetDesktop0" : "MsgSetLock0"));
            }
        }

        private async void DownloadAsync() {
            StorageFile file = await provider.Download(meta, resLoader.GetString("AppNameShort"),
                resLoader.GetString("Provider_" + provider.Id));
            if (file != null) {
                ToggleInfo(null, resLoader.GetString("MsgSave1"), InfoBarSeverity.Success, resLoader.GetString("ActionGo"), () => {
                    LaunchPicLib(file);
                    ToggleInfo(null, null);
                });
            } else {
                ToggleInfo(null, resLoader.GetString("MsgSave0"));
            }
        }

        private async void LaunchPicLib(StorageFile fileSelected) {
            try {
                var folder = await KnownFolders.PicturesLibrary.GetFolderAsync(resLoader.GetString("AppNameShort"));
                FolderLauncherOptions options = new FolderLauncherOptions();
                if (fileSelected != null) { // 打开文件夹同时选中目标文件
                    options.ItemsToSelect.Add(fileSelected);
                }
                _ = await Launcher.LaunchFolderAsync(folder, options);
            } catch (Exception) {
                Debug.WriteLine("launch folder failed");
            }
        }

        private async void LaunchRelealse() {
            try {
                _ = await Launcher.LaunchUriAsync(new Uri(release?.Url));
            } catch (Exception) {
                Debug.WriteLine("launch url failed");
            }
        }

        private void ToggleFullscreenMode() {
            ToggleFullscreenMode(!ApplicationView.GetForCurrentView().IsFullScreenMode);
        }

        private void ToggleFullscreenMode(bool fullScreen) {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (fullScreen) {
                if (view.TryEnterFullScreenMode()) {
                    ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                }
            } else {
                view.ExitFullScreenMode();
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
            }
        }

        private void ToggleInfo(string title, string msg, InfoBarSeverity severity = InfoBarSeverity.Error,
            string action = null, BtnInfoLinkHandler handler = null) {
            if (string.IsNullOrEmpty(msg)) {
                Info.IsOpen = false;
                return;
            }
            Info.Severity = severity;
            Info.Title = title ?? "";
            Info.Message = msg;
            InfoLink = handler;
            BtnInfoLink.Content = action ?? resLoader.GetString("ActionGo");
            BtnInfoLink.Visibility = handler != null ? Visibility.Visible : Visibility.Collapsed;
            Info.IsOpen = true;
        }

        private void ToggleImgMode(bool fillOn) {
            ImgUhd.Stretch = fillOn ? Stretch.UniformToFill : Stretch.Uniform;
            MpeUhd.Stretch = fillOn ? Stretch.UniformToFill : Stretch.Uniform;
            MenuFillOn.Visibility = fillOn ? Visibility.Collapsed : Visibility.Visible;
            MenuFillOff.Visibility = fillOn ? Visibility.Visible : Visibility.Collapsed;

            ToggleInfo(null, fillOn ? resLoader.GetString("MsgUniformToFill") : resLoader.GetString("MsgUniform"),
                InfoBarSeverity.Informational);
        }

        private async void CheckUpdateAsync() {
            release = await ApiService.CheckUpdate();
            if (release == null) {
                return;
            }
            await Task.Delay(2000);
            ToggleInfo(null, resLoader.GetString("MsgUpdate"), InfoBarSeverity.Informational, resLoader.GetString("ActionGo"), () => {
                LaunchRelealse();
                ToggleInfo(null, null);
            });
        }

        private async void ChecReviewAsync() {
            int.TryParse(localSettings.Values["launchTimes"]?.ToString(), out int times);
            localSettings.Values["launchTimes"] = ++times;
            if (times != 15) {
                return;
            }
            await Task.Delay(1000);
            FlyoutMenu.Hide();
            var action = await new ReviewDlg {
                RequestedTheme = ThemeUtil.ParseTheme(ini.Theme) // 修复未响应主题切换的BUG
            }.ShowAsync();
            if (action == ContentDialogResult.Primary) {
                _ = Launcher.LaunchUriAsync(new Uri(resLoader.GetStringForUri(new Uri("ms-resource:///Resources/LinkReview/NavigateUri"))));
            } else {
                localSettings.Values.Remove("launchTimes");
            }
        }

        private async Task<bool> RegService() {
            BackgroundAccessStatus reqStatus = await BackgroundExecutionManager.RequestAccessAsync();
            Debug.WriteLine("RequestAccessAsync: " + reqStatus);
            if (reqStatus != BackgroundAccessStatus.AlwaysAllowed
                && reqStatus != BackgroundAccessStatus.AllowedSubjectToSystemPolicy) {
                ToggleInfo(null, resLoader.GetString("TitleErrPush"));
                return false;
            }
            if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(BG_TASK_NAME_TIMER))) {
                Debug.WriteLine("service registered already");
                return true;
            }

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder {
                Name = BG_TASK_NAME_TIMER,
                TaskEntryPoint = typeof(PushService).FullName
            };
            // 触发任务的事件
            builder.SetTrigger(new TimeTrigger(60, false)); // 周期执行（不低于15min）
            // 触发任务的先决条件
            builder.AddCondition(new SystemCondition(SystemConditionType.SessionConnected)); // Internet 必须连接
            _ = builder.Register();

            Debug.WriteLine("service registered");
            return true;
        }

        private void UnregService() {
            foreach (var ta in BackgroundTaskRegistration.AllTasks) {
                if (ta.Value.Name == BG_TASK_NAME_TIMER) {
                    ta.Value.Unregister(true);
                    Debug.WriteLine("service BG_TASK_NAME_TIMER unregistered");
                } else if (ta.Value.Name == BG_TASK_NAME) {
                    ta.Value.Unregister(true);
                    Debug.WriteLine("service BG_TASK_NAME unregistered");
                }
            }
        }

        private async void RunServiceNow() {
            Debug.WriteLine("RunServiceNow()");

            ApplicationTrigger _AppTrigger = null;
            foreach (var task in BackgroundTaskRegistration.AllTasks) {
                if (task.Value.Name == BG_TASK_NAME) { // 已注册
                    _AppTrigger = (task.Value as BackgroundTaskRegistration).Trigger as ApplicationTrigger;
                    break;
                }
            }
            if (_AppTrigger == null) { // 后台任务从未注册过
                _AppTrigger = new ApplicationTrigger();

                BackgroundTaskBuilder builder = new BackgroundTaskBuilder {
                    Name = BG_TASK_NAME,
                    TaskEntryPoint = typeof(PushService).FullName
                };
                builder.SetTrigger(_AppTrigger);
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                _ = builder.Register();
            }
            _ = await _AppTrigger.RequestAsync();
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e) {
            if (resizeTimer == null) {
                resizeTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1500) };
                resizeTimer.Tick += (sender2, e2) => {
                    resizeTimer.Stop();
                    ReDecodeImg();
                };
            }
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void MenuYesterday_Click(object sender, RoutedEventArgs e) {
            AnimeYesterday1.Begin();
            StatusLoading();
            LoadYesterdayAsync();
        }

        private void MenuSetDesktop_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();
            SetWallpaperAsync(meta, true);
            ApiService.Rank(ini, meta, "desktop");
            ChecReviewAsync();
        }

        private void MenuSetLock_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();
            SetWallpaperAsync(meta, false);
            ApiService.Rank(ini, meta, "lock");
            ChecReviewAsync();
        }

        private void MenuVolumn_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();
            MpeUhd.MediaPlayer.Volume = MpeUhd.MediaPlayer.Volume > 0 ? 0 : 0.5;
            MenuVolumnOn.Visibility = MpeUhd.MediaPlayer.Volume == 0 ? Visibility.Visible : Visibility.Collapsed;
            MenuVolumnOff.Visibility = MpeUhd.MediaPlayer.Volume > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MenuFill_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();

            ToggleImgMode(ImgUhd.Stretch != Stretch.UniformToFill);
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();
            DownloadAsync();
            ApiService.Rank(ini, meta, "save");
            ChecReviewAsync();
        }

        private void MenuDislike_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();
            ApiService.Rank(ini, meta, "dislike");
            ToggleInfo(null, resLoader.GetString("MsgDislike"), InfoBarSeverity.Success, resLoader.GetString("ActionUndo"), () => {
                ToggleInfo(null, null);
                ApiService.Rank(ini, meta, "dislike", true);
            });
        }

        private void MenuPush_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();

            AppBarButton menuCheck = sender as AppBarButton;
            if (MenuPushDesktop.Tag.Equals(menuCheck.Tag)) {
                if (MenuPushDesktopIcon.Visibility == Visibility.Visible) {
                    MenuPushDesktopIcon.Visibility = Visibility.Collapsed;
                } else {
                    MenuPushDesktopIcon.Visibility = Visibility.Visible;
                    MenuCurDesktopIcon.Visibility = Visibility.Collapsed;
                    MenuCurDesktop.Visibility = Visibility.Collapsed;
                }
            } else if (MenuCurDesktop.Tag.Equals(menuCheck.Tag)) {
                MenuCurDesktopIcon.Visibility = Visibility.Collapsed;
                MenuCurDesktop.Visibility = Visibility.Collapsed;
            }
            if (MenuPushLock.Tag.Equals(menuCheck.Tag)) {
                if (MenuPushLockIcon.Visibility == Visibility.Visible) {
                    MenuPushLockIcon.Visibility = Visibility.Collapsed;
                } else {
                    MenuPushLockIcon.Visibility = Visibility.Visible;
                    MenuCurLockIcon.Visibility = Visibility.Collapsed;
                    MenuCurLock.Visibility = Visibility.Collapsed;
                }
            } else if (MenuCurLock.Tag.Equals(menuCheck.Tag)) {
                MenuCurLockIcon.Visibility = Visibility.Collapsed;
                MenuCurLock.Visibility = Visibility.Collapsed;
            }

            if (MenuCurDesktopIcon.Visibility == Visibility.Collapsed) {
                ini.DesktopProvider = MenuPushDesktopIcon.Visibility == Visibility.Visible ? provider.Id : "";
                IniUtil.SaveDesktopProvider(ini.DesktopProvider);
            }
            if (MenuCurLockIcon.Visibility == Visibility.Collapsed) {
                ini.LockProvider = MenuPushLockIcon.Visibility == Visibility.Visible ? provider.Id : "";
                IniUtil.SaveLockProvider(ini.LockProvider);
            }

            if (string.IsNullOrEmpty(ini.DesktopProvider) && string.IsNullOrEmpty(ini.LockProvider)) {
                UnregService();
            } else {
                _ = RegService();
                if ((MenuPushDesktop.Tag.Equals(menuCheck.Tag) && MenuPushDesktopIcon.Visibility == Visibility.Visible)
                    || (MenuPushLock.Tag.Equals(menuCheck.Tag) && MenuPushLockIcon.Visibility == Visibility.Visible)) {
                    RunServiceNow(); // 用户浏览图源与推送图源一致，立即推送一次
                }
            }

            if (MenuPushDesktop.Tag.Equals(menuCheck.Tag)) {
                if (MenuPushDesktopIcon.Visibility == Visibility.Visible) {
                    ToggleInfo(null, resLoader.GetString("MsgPushDesktopOn"), InfoBarSeverity.Success);
                } else {
                    ToggleInfo(null, resLoader.GetString("MsgPushDesktopOff"), InfoBarSeverity.Warning);
                }
            }
            if (MenuCurDesktop.Tag.Equals(menuCheck.Tag)) {
                ToggleInfo(null, resLoader.GetString("MsgPushDesktopOff"), InfoBarSeverity.Warning);
            }
            if (MenuPushLock.Tag.Equals(menuCheck.Tag)) {
                if (MenuPushLockIcon.Visibility == Visibility.Visible) {
                    ToggleInfo(null, resLoader.GetString("MsgPushLockOn"), InfoBarSeverity.Success);
                } else {
                    ToggleInfo(null, resLoader.GetString("MsgPushLockOff"), InfoBarSeverity.Warning);
                }
            }
            if (MenuCurLock.Tag.Equals(menuCheck.Tag)) {
                ToggleInfo(null, resLoader.GetString("MsgPushLockOff"), InfoBarSeverity.Warning);
            }
        }

        private void MenuProvider_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();

            IniUtil.SaveProvider(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();
            ViewSettings.BeforePaneOpen(ini);
            ViewSplit.IsPaneOpen = true;
        }

        private void BtnInfoLink_Click(object sender, RoutedEventArgs e) {
            InfoLink?.Invoke();
        }

        private void ViewBarPointer_PointerEntered(object sender, PointerRoutedEventArgs e) {
            ProgressLoading.Visibility = Visibility.Visible;
            ViewStory.Visibility = Visibility.Visible;
            ToggleInfo(null, null);
        }

        private void ViewBarPointer_PointerExited(object sender, PointerRoutedEventArgs e) {
            if (ProgressLoading.ShowPaused && !ProgressLoading.ShowError) {
                ProgressLoading.Visibility = Visibility.Collapsed;
            }
            ViewStory.Visibility = Visibility.Collapsed;
            ToggleInfo(null, null);
        }

        private void ImgUhd_ImageOpened(object sender, RoutedEventArgs e) {
            StatusEnjoy();
        }

        private void ImgUhd_Tapped(object sender, TappedRoutedEventArgs e) {
            ViewSplit.IsPaneOpen = false;
        }

        private void ImgUhd_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {
            ToggleFullscreenMode();
        }

        private void ImgUhd_PointerWheelChanged(object sender, PointerRoutedEventArgs e) {
            if (stretchTimer == null) {
                stretchTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 400) };
                stretchTimer.Tick += (sender2, e2) => {
                    stretchTimer.Stop();
                    ToggleImgMode(ImgUhd.Stretch != Stretch.UniformToFill);
                };
            }
            stretchTimer.Stop();
            stretchTimer.Start();
        }

        private void BoxGo_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == VirtualKey.Enter) {
                FlyoutGo.Hide();
                DateTime date = DateUtil.ParseDate(BoxGo.Text);
                if (date.Date != meta?.Date?.Date) {
                    StatusLoading();
                    LoadTargetAsync(date);
                }
            }
        }

        private void FlyoutMenu_Opened(object sender, object e) {
            localSettings.Values["MenuLearned"] = true;
            ToggleInfo(null, null);
        }

        private void KeyInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
            switch (sender.Key) {
                case VirtualKey.Left:
                case VirtualKey.Up:
                    StatusLoading();
                    LoadYesterdayAsync();
                    break;
                case VirtualKey.Right:
                case VirtualKey.Down:
                    StatusLoading();
                    LoadTormorrowAsync();
                    break;
                case VirtualKey.Enter:
                    ToggleFullscreenMode(true);
                    break;
                case VirtualKey.Escape:
                    ToggleFullscreenMode(false);
                    break;
                case VirtualKey.B:
                    if (sender.Modifiers == VirtualKeyModifiers.Control) {
                        MenuSetDesktop_Click(null, null);
                    }
                    break;
                case VirtualKey.L:
                    if (sender.Modifiers == VirtualKeyModifiers.Control) {
                        MenuSetLock_Click(null, null);
                    }
                    break;
                case VirtualKey.S:
                    if (sender.Modifiers == VirtualKeyModifiers.Control) {
                        MenuSave_Click(null, null);
                    }
                    break;
                case VirtualKey.D:
                    if (sender.Modifiers == VirtualKeyModifiers.Control) {
                        MenuDislike_Click(null, null);
                    }
                    break;
                case VirtualKey.R:
                    if (sender.Modifiers == VirtualKeyModifiers.Control) {
                        FlyoutMenu.Hide();
                        Refresh(true);
                    }
                    break;
                case VirtualKey.F5:
                    FlyoutMenu.Hide();
                    Refresh();
                    break;
                case VirtualKey.F:
                case VirtualKey.G:
                    if (ini.GetIni().IsSequential() && !FlyoutGo.IsOpen) {
                        BoxGo.PlaceholderText = string.Format(resLoader.GetString("CurDate"), meta?.Date?.ToString("M") ?? "MMdd");
                        BoxGo.Text = "";
                        FlyoutBase.ShowAttachedFlyout(AnchorGo);
                    } else {
                        FlyoutGo.Hide();
                    }
                    break;
            }
            args.Handled = true;
        }

        private void Refresh(bool useCtrlR = false) {
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();

            if (!localSettings.Values.ContainsKey("tipRefresh")) {
                localSettings.Values["tipRefresh"] = true;
                ToggleInfo(resLoader.GetString("MsgKey"), useCtrlR ? resLoader.GetString("MsgCtrlR") : resLoader.GetString("MsgF5"),
                    InfoBarSeverity.Informational);
            }
        }

        private void AnimeYesterday1_Completed(object sender, object e) {
            AnimeYesterday2.Begin();
        }

        private void MenuSettings_PointerEntered(object sender, PointerRoutedEventArgs e) {
            AnimeSettings.Begin();
        }

        private void MenuDislike_PointerEntered(object sender, PointerRoutedEventArgs e) {
            AnimeDislike.Begin();
        }

        private void ViewSettings_SettingsChanged(object sender, SettingsEventArgs e) {
            if (e.ProviderChanged) {
                ini = null;
                provider = null;
                StatusLoading();
                LoadFocusAsync();
            }
            if (e.ThemeChanged) { // 修复 muxc:CommandBarFlyout.SecondaryCommands 子元素无法响应随主题改变的BUG
                ElementTheme theme = ThemeUtil.ParseTheme(ini.Theme);
                MenuProvider.RequestedTheme = theme;
                MenuSetDesktop.RequestedTheme = theme;
                MenuSetLock.RequestedTheme = theme;
                MenuPushDesktop.RequestedTheme = theme;
                MenuCurDesktop.RequestedTheme = theme;
                MenuPushLock.RequestedTheme = theme;
                MenuCurLock.RequestedTheme = theme;
                Separator1.RequestedTheme = theme;
                Separator2.RequestedTheme = theme;
                Separator3.RequestedTheme = theme;
                foreach (RadioMenuFlyoutItem item in SubmenuProvider.Items.Cast<RadioMenuFlyoutItem>()) {
                    item.RequestedTheme = theme;
                }
                // 刷新状态颜色
                ProgressLoading.ShowError = !ProgressLoading.ShowError;
                ProgressLoading.ShowError = !ProgressLoading.ShowError;
            }
        }

        private async void ViewSettings_ContributeChanged(object sender, EventArgs e) {
            ContributeDlg dlg = new ContributeDlg {
                RequestedTheme = ThemeUtil.ParseTheme(ini.Theme) // 修复未响应主题切换的BUG
            };
            var res = await dlg.ShowAsync();
            if (res == ContentDialogResult.Primary) {
                ContributeApiReq req = dlg.GetContent();
                bool status = await ApiService.Contribute(req);
                if (status) {
                    ToggleInfo(null, resLoader.GetString("MsgContribute1"), InfoBarSeverity.Success);
                } else {
                    ToggleInfo(null, resLoader.GetString("MsgContribute0"), InfoBarSeverity.Warning);
                }
            }
        }

        private void MenuFillOff_PointerEntered(object sender, PointerRoutedEventArgs e) {
            AnimeFillOff.Begin();
        }
    }
}
