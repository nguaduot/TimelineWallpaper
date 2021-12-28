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
            Debug.WriteLine("meta: " + JsonConvert.SerializeObject(meta).Trim());
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
            Debug.WriteLine("meta: " + JsonConvert.SerializeObject(meta).Trim());
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

        private async void LoadLastAsync() {
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

            MenuPushCur.Text = string.Format(resLoader.GetString("Pushing"), resLoader.GetString("Provider_" + ini.PushProvider));
            MenuPushDesktop.Text = string.Format(resLoader.GetString("PushDesktop"), resLoader.GetString("Provider_" + provider.Id));
            MenuPushLock.Text = string.Format(resLoader.GetString("PushLock"), resLoader.GetString("Provider_" + provider.Id));
            if (MenuPushNone.Tag.Equals(ini.Push)) {
                MenuPushNone.IsChecked = true;
                MenuPushCur.Visibility = Visibility.Collapsed;
                MenuPushDesktop.IsChecked = false;
                MenuPushLock.IsChecked = false;
            } else {
                MenuPushNone.IsChecked = false;
                if (provider.Id.Equals(ini.PushProvider)) {
                    MenuPushCur.Visibility = Visibility.Collapsed;
                    MenuPushDesktop.IsChecked = MenuPushDesktop.Tag.Equals(ini.Push);
                    MenuPushLock.IsChecked = MenuPushLock.Tag.Equals(ini.Push);
                } else {
                    MenuPushCur.Visibility = Visibility.Visible;
                    MenuPushCur.Tag = ini.Push;
                    MenuPushCur.IsChecked = true;
                    MenuPushDesktop.IsChecked = false;
                    MenuPushLock.IsChecked = false;
                }
            }
            if (MenuPushNone.IsChecked) {
                UnregService();
            } else {
                _ = RegService();
                if (provider.Id.Equals(ini.PushProvider)) {
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
            // 位置
            TextDetailLocation.Text = meta.Location ?? "";
            // 图文故事
            TextDetailStory.Text = meta.Story ?? "";
            // 版权所有者
            TextDetailCopyright.Text = meta.Copyright ?? "";
            // 日期
            TextDetailDate.Text = meta.Date?.ToLongDateString();
            // 文件属性
            TextDetailProperties.Text = "";

            //if (ini.GetIni().IsSequential()) {
            //    CalendarGo.SelectedDates.Clear();
            //    CalendarGo.SelectedDates.Add(meta.Date.Value);
            //    CalendarGo.SetDisplayDate(meta.Date.Value.AddDays(-7));
            //} else {
            //    CalendarGo.Visibility = Visibility.Collapsed;
            //}
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
                if (meta.Dimen.Width / meta.Dimen.Height > winW / winH) {
                    biUhd.DecodePixelWidth = (int)Math.Round(winH * meta.Dimen.Width / meta.Dimen.Height);
                    biUhd.DecodePixelHeight = (int)Math.Round(winH);
                } else {
                    biUhd.DecodePixelWidth = (int)Math.Round(winW);
                    biUhd.DecodePixelHeight = (int)Math.Round(winW * meta.Dimen.Height / meta.Dimen.Width);
                }
                biUhd.UriSource = new Uri(meta.CacheUhd.Path, UriKind.Absolute);
            } else {
                ImgUhd.Source = null;
            }

            StorageFile file = meta.CacheUhd ?? meta.CacheVideo ?? meta.CacheAudio;
            string fileSize = FileUtil.ConvertFileSize(file != null ? new FileInfo(file.Path).Length : 0);
            TextDetailProperties.Text = string.Format(resLoader.GetString("DetailSize"),
                resLoader.GetString("Provider_" + provider.Id), meta.Dimen.Width, meta.Dimen.Height, fileSize);
            MenuSetDesktop.IsEnabled = meta.CacheUhd != null;
            MenuSetLock.IsEnabled = meta.CacheUhd != null;
            MenuVolumnOn.Visibility = (meta.CacheVideo != null || meta.CacheAudio != null) && MpeUhd.MediaPlayer.Volume == 0
                ? Visibility.Visible : Visibility.Collapsed;
            MenuVolumnOff.Visibility = (meta.CacheVideo != null || meta.CacheAudio != null) && MpeUhd.MediaPlayer.Volume > 0
                ? Visibility.Visible : Visibility.Collapsed;
            MenuSave.IsEnabled = meta.CacheUhd != null || meta.CacheVideo != null || meta.CacheAudio != null;

            StatusEnjoy();
        }

        private void ReDecodeImg() {
            if (ImgUhd.Source == null) {
                return;
            }
            float winW = Window.Current.Content.ActualSize.X;
            float winH = Window.Current.Content.ActualSize.Y;
            BitmapImage bi = ImgUhd.Source as BitmapImage;
            if (bi.PixelHeight == 0) {
                Debug.WriteLine("ReDecodeImg(): bi.PixelWidth 0");
                return;
            }
            bi.DecodePixelType = DecodePixelType.Logical;
            if (bi.PixelWidth / bi.PixelHeight > winW / winH) {
                bi.DecodePixelWidth = (int)Math.Round(winH * bi.PixelWidth / bi.PixelHeight);
                bi.DecodePixelHeight = (int)Math.Round(winH);
            } else {
                bi.DecodePixelWidth = (int)Math.Round(winW);
                bi.DecodePixelHeight = (int)Math.Round(winW * bi.PixelHeight / bi.PixelWidth);
            }
        }

        private void StatusEnjoy() {
            ImgUhd.Opacity = 1;
            ImgUhd.Scale = new Vector3(1, 1, 1);
            MpeUhd.Opacity = 1;
            MpeUhd.Scale = new Vector3(1, 1, 1);
            ProgressLoading.ShowPaused = true;
            ProgressLoading.ShowError = false;
            ProgressLoading.Visibility = TextDetailDate.Visibility;
        }

        private void StatusLoading() {
            ImgUhd.Opacity = 0;
            ImgUhd.Scale = new Vector3(1.014f, 1.014f, 1.014f);
            MpeUhd.Opacity = 0;
            MpeUhd.Scale = new Vector3(1.014f, 1.014f, 1.014f);
            ProgressLoading.ShowPaused = false;
            ProgressLoading.ShowError = false;
            ProgressLoading.Visibility = Visibility.Visible;
            ToggleInfo(null);
        }

        private void StatusError() {
            ImgUhd.Opacity = 0;
            MpeUhd.Opacity = 0;
            TextTitle.Text = resLoader.GetString("AppDesc");
            TextDetailCaption.Text = "";
            TextDetailLocation.Text = "";
            TextDetailStory.Text = "";
            TextDetailCopyright.Text = "";
            TextDetailDate.Text = "";
            TextDetailProperties.Text = "";
            ProgressLoading.ShowError = true;
            ProgressLoading.Visibility = Visibility.Visible;

            MenuSetDesktop.IsEnabled = false;
            MenuSetLock.IsEnabled = false;
            MenuVolumnOn.Visibility = Visibility.Collapsed;
            MenuVolumnOff.Visibility = Visibility.Collapsed;
            MenuSave.IsEnabled = false;

            ToggleInfo(!NetworkInterface.GetIsNetworkAvailable() ? resLoader.GetString("MsgNoInternet")
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
                ToggleInfo(resLoader.GetString("MsgWallpaper0"));
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
                ToggleInfo(resLoader.GetString(setDesktopOrLock ? "MsgSetDesktop1" : "MsgSetLock1"), InfoBarSeverity.Success);
            } else {
                ToggleInfo(resLoader.GetString(setDesktopOrLock ? "MsgSetDesktop0" : "MsgSetLock0"));
            }
        }

        private async void DownloadAsync() {
            StorageFile file = await provider.Download(meta, resLoader.GetString("AppNameShort"),
                resLoader.GetString("Provider_" + provider.Id));
            if (file != null) {
                ToggleInfo(resLoader.GetString("MsgSave1"), InfoBarSeverity.Success, () => {
                    LaunchPicLib(file);
                    ToggleInfo(null);
                });
            } else {
                ToggleInfo(resLoader.GetString("MsgSave0"));
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

        private void ToggleStory(bool on) {
            if (on) {
                TextDetailCaption.Visibility = TextDetailCaption.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                TextDetailLocation.Visibility = TextDetailLocation.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                TextDetailStory.Visibility = TextDetailStory.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                TextDetailCopyright.Visibility = TextDetailCopyright.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                TextDetailDate.Visibility = TextDetailDate.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                TextDetailProperties.Visibility = TextDetailProperties.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            } else {
                TextDetailCaption.Visibility = Visibility.Collapsed;
                TextDetailLocation.Visibility = Visibility.Collapsed;
                TextDetailStory.Visibility = Visibility.Collapsed;
                TextDetailCopyright.Visibility = Visibility.Collapsed;
                TextDetailDate.Visibility = Visibility.Collapsed;
                TextDetailProperties.Visibility = Visibility.Collapsed;
            }
        }

        private void ToggleInfo(string msg, InfoBarSeverity severity = InfoBarSeverity.Error, BtnInfoLinkHandler handler = null) {
            if (string.IsNullOrEmpty(msg)) {
                Info.IsOpen = false;
                return;
            }
            Info.Severity = severity;
            Info.Message = msg;
            InfoLink = handler;
            BtnInfoLink.Visibility = handler != null ? Visibility.Visible : Visibility.Collapsed;
            Info.IsOpen = true;
        }

        private async void CheckUpdateAsync() {
            release = await ApiService.CheckUpdate();
            if (release == null) {
                return;
            }
            await Task.Delay(2000);
            ToggleInfo(resLoader.GetString("MsgUpdate"), InfoBarSeverity.Informational, () => {
                LaunchRelealse();
                ToggleInfo(null);
            });
        }

        private async void ChecReviewAsync() {
            int.TryParse(localSettings.Values["launchTimes"]?.ToString(), out int times);
            localSettings.Values["launchTimes"] = ++times;
            if (times != 10) {
                return;
            }
            await Task.Delay(1000);
            //localSettings.Values.Remove("launchTimes");
            FlyoutMenu.Hide();
            _ = new ReviewDlg().ShowAsync();
        }

        private async Task<bool> RegService() {
            BackgroundAccessStatus reqStatus = await BackgroundExecutionManager.RequestAccessAsync();
            Debug.WriteLine("RequestAccessAsync: " + reqStatus);
            if (reqStatus != BackgroundAccessStatus.AlwaysAllowed
                && reqStatus != BackgroundAccessStatus.AllowedSubjectToSystemPolicy) {
                ToggleInfo(resLoader.GetString("TitleErrPush"));
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
            ChecReviewAsync();
        }

        private void MenuSetLock_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();
            SetWallpaperAsync(meta, false);
            ChecReviewAsync();
        }

        private void MenuVolumn_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();
            MpeUhd.MediaPlayer.Volume = MpeUhd.MediaPlayer.Volume > 0 ? 0 : 0.5;
            MenuVolumnOn.Visibility = MpeUhd.MediaPlayer.Volume == 0 ? Visibility.Visible : Visibility.Collapsed;
            MenuVolumnOff.Visibility = MpeUhd.MediaPlayer.Volume > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();
            DownloadAsync();
            ChecReviewAsync();
        }

        private void MenuPush_Click(object sender, RoutedEventArgs e) {
            FlyoutMenu.Hide();

            string push = ((ToggleMenuFlyoutItem)sender).Tag.ToString();
            MenuPushNone.IsChecked = MenuPushNone.Tag.Equals(push);
            MenuPushDesktop.IsChecked = MenuPushDesktop.Tag.Equals(push);
            MenuPushLock.IsChecked = MenuPushLock.Tag.Equals(push);
            MenuPushCur.Visibility = Visibility.Collapsed; // 该项禁选，因此无论选择其他任何项，该项都不再显示

            IniUtil.SavePush(push);
            IniUtil.SavePushProvider(provider.Id);
            ini.Push = push;
            ini.PushProvider = provider.Id;
            if (MenuPushNone.IsChecked) {
                UnregService();
            } else {
                _ = RegService();
                RunServiceNow(); // 用户明确开启推送，立即推送一次
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

        private void MenuBingLang_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveBingLang(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void MenuNasaMirror_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveNasaMirror(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void MenuOneplusOrder_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveOneplusOrder(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void MenuTimelineOrder_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveTimelineOrder(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void MenuTimelineCate_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveTimelineCate(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void MenuYmyouliCol_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveYmyouliCol(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void MenuInfinityOrder_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveInfinityOrder(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e) {
            //AnimeSettings.Begin();
            ViewSplit.IsPaneOpen = true;
        }

        private void BtnInfoLink_Click(object sender, RoutedEventArgs e) {
            InfoLink?.Invoke();
        }

        private void ViewBar_PointerEntered(object sender, PointerRoutedEventArgs e) {
            ProgressLoading.Visibility = Visibility.Visible;
            ToggleStory(true);
            ToggleInfo(null);
        }

        private void ViewBar_PointerExited(object sender, PointerRoutedEventArgs e) {
            if (ProgressLoading.ShowPaused && !ProgressLoading.ShowError) {
                ProgressLoading.Visibility = Visibility.Collapsed;
            }
            ToggleStory(false);
            ToggleInfo(null);
        }

        private void ImgUhd_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {
            ToggleFullscreenMode();
        }

        private void ImgUhd_PointerWheelChanged(object sender, PointerRoutedEventArgs e) {
            if (stretchTimer == null) {
                stretchTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 400) };
                stretchTimer.Tick += (sender2, e2) => {
                    stretchTimer.Stop();
                    ImgUhd.Stretch = ImgUhd.Stretch == Stretch.Uniform ? Stretch.UniformToFill : Stretch.Uniform;
                    MpeUhd.Stretch = MpeUhd.Stretch == Stretch.Uniform ? Stretch.UniformToFill : Stretch.Uniform;
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
                    LoadLastAsync();
                    break;
                case VirtualKey.Enter:
                    ToggleFullscreenMode(true);
                    break;
                case VirtualKey.Escape:
                    ToggleFullscreenMode(false);
                    break;
                case VirtualKey.B:
                    if (sender.Modifiers == VirtualKeyModifiers.Control) {
                        SetWallpaperAsync(meta, true);
                        ChecReviewAsync();
                    }
                    break;
                case VirtualKey.L:
                    if (sender.Modifiers == VirtualKeyModifiers.Control) {
                        SetWallpaperAsync(meta, false);
                        ChecReviewAsync();
                    }
                    break;
                case VirtualKey.S:
                    if (sender.Modifiers == VirtualKeyModifiers.Control) {
                        DownloadAsync();
                        ChecReviewAsync();
                    }
                    break;
                case VirtualKey.R:
                    if (sender.Modifiers == VirtualKeyModifiers.Control) {
                        ini = null;
                        provider = null;
                        StatusLoading();
                        LoadFocusAsync();
                    }
                    break;
                case VirtualKey.F5:
                    ini = null;
                    provider = null;
                    StatusLoading();
                    LoadFocusAsync();
                    break;
                case VirtualKey.F:
                case VirtualKey.G:
                    if (ini.GetIni().IsSequential() && !FlyoutGo.IsOpen) {
                        BoxGo.PlaceholderText = String.Format(resLoader.GetString("CurDate"), meta.Date?.ToString("M") ?? "MMdd");
                        BoxGo.Text = "";
                        FlyoutBase.ShowAttachedFlyout(AnchorGo);
                    } else {
                        FlyoutGo.Hide();
                    }
                    break;
            }
            args.Handled = true;
        }

        private void AnimeYesterday1_Completed(object sender, object e) {
            AnimeYesterday2.Begin();
        }

        private void ImgUhd_Tapped(object sender, TappedRoutedEventArgs e) {
            ViewSplit.IsPaneOpen = false;
        }

        private void MenuSettings_PointerEntered(object sender, PointerRoutedEventArgs e) {
            AnimeSettings.Begin();
        }

        private void ViewSplit_PaneOpened(SplitView sender, object args) {
            ViewSettings.PaneOpened(ini);
        }

        private void ViewSettings_SettingsChanged(object sender, SettingsEventArgs e) {
            if (e.CurProviderIniChanged) {
                ini = null;
                provider = null;
                StatusLoading();
                LoadFocusAsync();
            }
            if (e.ThemeChanged) { // 修复 muxc:CommandBarFlyout.SecondaryCommands 子元素无法响应随主题改变的BUG
                ElementTheme theme = ThemeUtil.ParseTheme(ini.Theme);
                MenuProvider.RequestedTheme = theme;
                MenuPush.RequestedTheme = theme;
                MenuSettings.RequestedTheme = theme;
                foreach (RadioMenuFlyoutItem item in SubmenuProvider.Items.Cast<RadioMenuFlyoutItem>()) {
                    item.RequestedTheme = theme;
                }
                foreach (ToggleMenuFlyoutItem item in SubmenuPush.Items.Cast<ToggleMenuFlyoutItem>()) {
                    item.RequestedTheme = theme;
                }
            }
        }
    }
}
