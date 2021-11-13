using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Threading.Tasks;
using TimelineWallpaper.Beans;
using TimelineWallpaper.Providers;
using TimelineWallpaper.Utils;
using TWPushService;
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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace TimelineWallpaper {
    public sealed partial class MainPage : Page {
        private readonly ResourceLoader resLoader;

        private Ini ini = null;
        private BaseProvider provider = null;
        private Meta meta = null;

        private DispatcherTimer resizeTimer = null;

        private const string BG_TASK_NAME = "PushTask";
        private const string BG_TASK_NAME_TIMER = "PushTaskTimer";
        private const int MIN_COST_OF_LOAD = 800;

        public MainPage() {
            this.InitializeComponent();

            resLoader = ResourceLoader.GetForCurrentView();
            Init();
            LoadFocusAsync();
        }

        private void Init() {
            ImgUhd.MediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
            ImgUhd.MediaPlayer.IsLoopingEnabled = true;
            ImgUhd.MediaPlayer.Volume = 0;

            resizeTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1500) };
            resizeTimer.Tick += ResizeTimer_Tick;
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        private void ResizeTimer_Tick(object sender, object e) {
            Debug.WriteLine("ResizeTimer_Tick " + DateTime.Now);
            resizeTimer.Stop();
            ReDecodeImg();
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e) {
            Debug.WriteLine("Current_SizeChanged " + DateTime.Now);
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void BtnSetDesktop_Click(object sender, RoutedEventArgs e) {
            SetWallpaperAsync(meta, true);
        }

        private void BtnSetLock_Click(object sender, RoutedEventArgs e) {
            SetWallpaperAsync(meta, false);
        }

        private void BtnVolumn_Click(object sender, RoutedEventArgs e) {
            ImgUhd.MediaPlayer.Volume = ImgUhd.MediaPlayer.Volume > 0 ? 0 : 0.5;
            BtnVolumnOn.Visibility = ImgUhd.MediaPlayer.Volume == 0 ? Visibility.Visible : Visibility.Collapsed;
            BtnVolumnOff.Visibility = ImgUhd.MediaPlayer.Volume > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e) {
            DownloadAsync();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e) {
            StatusLoading();
            LoadNextAsync();
        }

        private void BtnPush_Click(object sender, RoutedEventArgs e) {
            string push = ((RadioMenuFlyoutItem)sender).Tag.ToString();
            IniUtil.SavePush(push);
            ini.Push = push;
            if (BtnPushNone.IsChecked) {
                UnregService();
            } else {
                _ = RegService();
                RunServiceNow();
            }
        }

        private void BtnProvider_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveProvider(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void BtnBingLang_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveBingLang(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void BtnNasaMirror_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveNasaMirror(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void BtnOneplusOrder_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveOneplusOrder(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void Btn3GOrder_Click(object sender, RoutedEventArgs e) {
            IniUtil.Save3GOrder(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void BtnYmyouliCol_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveYmyouliCol(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void BtnInfinityOrder_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveInfinityOrder(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void BtnIni_Click(object sender, RoutedEventArgs e) {
            LaunchIni();
        }

        private void BtnLibPic_Click(object sender, RoutedEventArgs e) {
            LaunchPicLib();
            ToggleInfo(null);
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

        //private void ImgUhd_DragStarting(UIElement sender, DragStartingEventArgs args) {
        //    // Xmal:
        //    // CanDrag = "True"
        //    // DragStarting = "ImgUhd_DragStarting"
        //    args.DragUI.SetContentFromDataPackage();
        //    //args.DragUI.SetContentFromBitmapImage(new BitmapImage(new Uri("ms-appx://Assets/StoreLogo.png")),
        //    //    new Windows.Foundation.Point(0, 0));
        //    args.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        //    args.Data.SetStorageItems(new[] { meta.CacheUhd });
        //}

        private void ImgUhd_PointerWheelChanged(object sender, PointerRoutedEventArgs e) {
            //bool forward = e.GetCurrentPoint(this).Properties.MouseWheelDelta > 0;
            ImgUhd.Stretch = ImgUhd.Stretch == Stretch.Uniform ? Stretch.UniformToFill : Stretch.Uniform;
        }

        private void KeyInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
            switch (sender.Key) {
                case VirtualKey.Up:
                    StatusLoading();
                    LoadNextAsync();
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
            }
            args.Handled = true;
        }

        private async void LoadFocusAsync() {
            _ = await InitProvider();
            if (!await provider.LoadData(ini)) {
                Debug.WriteLine("failed to load data");
                ShowText(null);
                return;
            }

            meta = provider.GetFocus();
            Debug.WriteLine("meta: " + meta);
            ShowText(meta);
            Meta metaCache = await BaseProvider.Cache(provider, meta);
            if (metaCache != null && metaCache.IsValid() && metaCache.Id == meta?.Id) {
                ShowImg(meta);
            }

            // 预加载
            PreLoadNextAsync();
        }

        private async void LoadNextAsync() {
            long cost = DateTime.Now.Ticks;
            if (!await provider.LoadData(ini)) {
                Debug.WriteLine("failed to load data");
                if ((cost = DateTime.Now.Ticks - cost) / 10000 < MIN_COST_OF_LOAD) {
                    await Task.Delay(MIN_COST_OF_LOAD - (int)(cost / 10000));
                }
                ShowText(null);
                return;
            }

            meta = provider.GetNext();
            Debug.WriteLine("meta: " + meta);
            ShowText(meta);
            Meta metaCache = await BaseProvider.Cache(provider, meta);
            if ((cost = DateTime.Now.Ticks - cost) / 10000 < MIN_COST_OF_LOAD) {
                await Task.Delay(MIN_COST_OF_LOAD - (int)(cost / 10000));
            }
            if (metaCache != null && metaCache.IsValid() && metaCache.Id == meta?.Id) {
                ShowImg(meta);
            }

            // 预加载
            PreLoadNextAsync();
        }

        private async void LoadLastAsync() {
            long cost = DateTime.Now.Ticks;
            if (!await provider.LoadData(ini)) {
                Debug.WriteLine("failed to load data");
                if ((cost = DateTime.Now.Ticks - cost) / 10000 < MIN_COST_OF_LOAD) {
                    await Task.Delay(MIN_COST_OF_LOAD - (int)(cost / 10000));
                }
                ShowText(null);
                return;
            }

            meta = provider.GetLast();
            Debug.WriteLine("meta: " + meta);
            ShowText(meta);
            Meta metaCache = await BaseProvider.Cache(provider, meta);
            if ((cost = DateTime.Now.Ticks - cost) / 10000 < MIN_COST_OF_LOAD) {
                await Task.Delay(MIN_COST_OF_LOAD - (int)(cost / 10000));
            }
            if (metaCache != null && metaCache.IsValid() && metaCache.Id == meta?.Id) {
                ShowImg(meta);
            }
        }

        private async void PreLoadNextAsync() {
            if (await provider.LoadData(ini)) {
                _ = BaseProvider.Cache(provider, provider.GetNext(false));
            }
        }

        private async Task<bool> InitProvider() {
            if (ini == null) {
                ini = await IniUtil.GetIni();
            }
            if (provider != null && provider.Id.Equals(ini.Provider)) {
                return true;
            }

            provider = ini.GenerateProvider();

            BtnPushNone.IsChecked = BtnPushNone.Tag.Equals(ini.Push);
            BtnPushDesktop.IsChecked = BtnPushDesktop.Tag.Equals(ini.Push);
            BtnPushLock.IsChecked = BtnPushLock.Tag.Equals(ini.Push);
            if (BtnPushNone.IsChecked) {
                UnregService();
            } else {
                _ = await RegService();
                RunServiceNow();
            }

            BtnProviderBing.IsChecked = BtnProviderBing.Tag.Equals(ini.Provider);
            BtnBingLang.Visibility = BtnProviderBing.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            BtnProviderNasa.IsChecked = BtnProviderNasa.Tag.Equals(ini.Provider);
            BtnNasaMirror.Visibility = BtnProviderNasa.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            BtnProviderOneplus.IsChecked = BtnProviderOneplus.Tag.Equals(ini.Provider);
            BtnOneplusOrder.Visibility = BtnProviderOneplus.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            BtnProvider3G.IsChecked = BtnProvider3G.Tag.Equals(ini.Provider);
            Btn3GOrder.Visibility = BtnProvider3G.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            BtnProviderDaihan.IsChecked = BtnProviderDaihan.Tag.Equals(ini.Provider);
            BtnProviderYmyouli.IsChecked = BtnProviderYmyouli.Tag.Equals(ini.Provider);
            BtnYmyouliCol.Visibility = BtnProviderYmyouli.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            BtnProviderInfinity.IsChecked = BtnProviderInfinity.Tag.Equals(ini.Provider);
            BtnInfinityOrder.Visibility = BtnProviderInfinity.IsChecked ? Visibility.Visible : Visibility.Collapsed;

            BtnBingLangDef.IsChecked = BtnBingLangDef.Tag.Equals(ini.Bing.Lang);
            BtnBingLangCn.IsChecked = BtnBingLangCn.Tag.Equals(ini.Bing.Lang);
            BtnBingLangJp.IsChecked = BtnBingLangJp.Tag.Equals(ini.Bing.Lang);
            BtnBingLangJp.IsChecked = BtnBingLangJp.Tag.Equals(ini.Bing.Lang);
            BtnBingLangDe.IsChecked = BtnBingLangDe.Tag.Equals(ini.Bing.Lang);
            BtnBingLangFr.IsChecked = BtnBingLangFr.Tag.Equals(ini.Bing.Lang);
            BtnNasaMirrorDef.IsChecked = BtnNasaMirrorDef.Tag.Equals(ini.Nasa.Mirror);
            BtnNasaMirrorBjp.IsChecked = BtnNasaMirrorBjp.Tag.Equals(ini.Nasa.Mirror);
            BtnOneplusOrder1.IsChecked = BtnOneplusOrder1.Tag.Equals(ini.OnePlus.Order);
            BtnOneplusOrder2.IsChecked = BtnOneplusOrder2.Tag.Equals(ini.OnePlus.Order);
            BtnOneplusOrder3.IsChecked = BtnOneplusOrder3.Tag.Equals(ini.OnePlus.Order);
            Btn3GOrder1.IsChecked = Btn3GOrder1.Tag.Equals(ini.G3.Order);
            Btn3GOrder2.IsChecked = Btn3GOrder2.Tag.Equals(ini.G3.Order);
            BtnYmyouliColDef.IsChecked = BtnYmyouliColDef.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol126.IsChecked = BtnYmyouliCol126.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol182.IsChecked = BtnYmyouliCol182.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol183.IsChecked = BtnYmyouliCol183.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol215.IsChecked = BtnYmyouliCol215.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol184.IsChecked = BtnYmyouliCol184.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol185.IsChecked = BtnYmyouliCol184.Tag.Equals(ini.Ymyouli.Col);
            BtnInfinityOrder0.IsChecked = BtnInfinityOrder0.Tag.Equals(ini.Infinity.Order);
            BtnInfinityOrder1.IsChecked = BtnInfinityOrder1.Tag.Equals(ini.Infinity.Order);
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
            TextDetailDesc.Text = meta.Story ?? "";
            // 版权所有者
            TextDetailCopyright.Text = meta.Copyright ?? "";
            // 日期
            TextDetailDate.Text = meta.Date?.ToLongDateString();
            // 文件属性
            TextDetailProperties.Text = "";
        }

        private void ShowImg(Meta meta) {
            if (meta == null) {
                StatusError();
                return;
            }
            ImgUhd.Source = meta.CacheVideo != null ? MediaSource.CreateFromStorageFile(meta.CacheVideo) : null;
            if (meta.CacheUhd != null) {
                float winW = Window.Current.Content.ActualSize.X;
                float winH = Window.Current.Content.ActualSize.Y;
                BitmapImage biUhd = new BitmapImage();
                ImgUhd.PosterSource = biUhd;
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
                ImgUhd.PosterSource = null;
            }

            StorageFile file = meta.CacheUhd ?? meta.CacheVideo ?? meta.CacheAudio;
            string fileSize = FileUtil.ConvertFileSize(file != null ? new FileInfo(file.Path).Length : 0);
            TextDetailProperties.Text = string.Format(resLoader.GetString("DetailSize"),
                resLoader.GetString("Provider_" + provider.Id), meta.Dimen.Width, meta.Dimen.Height, fileSize);
            BtnSetDesktop.IsEnabled = meta.CacheUhd != null;
            BtnSetLock.IsEnabled = meta.CacheUhd != null;
            BtnVolumnOn.Visibility = (meta.CacheVideo != null || meta.CacheAudio != null) && ImgUhd.MediaPlayer.Volume == 0
                ? Visibility.Visible : Visibility.Collapsed;
            BtnVolumnOff.Visibility = (meta.CacheVideo != null || meta.CacheAudio != null) && ImgUhd.MediaPlayer.Volume > 0
                ? Visibility.Visible : Visibility.Collapsed;
            BtnSave.IsEnabled = meta.CacheUhd != null || meta.CacheVideo != null || meta.CacheAudio != null;

            StatusEnjoy();
        }

        private void ReDecodeImg() {
            if (ImgUhd.PosterSource == null) {
                return;
            }
            float winW = Window.Current.Content.ActualSize.X;
            float winH = Window.Current.Content.ActualSize.Y;
            BitmapImage bi = ImgUhd.PosterSource as BitmapImage;
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
            ProgressLoading.ShowPaused = true;
            ProgressLoading.ShowError = false;
            ProgressLoading.Visibility = TextDetailDate.Visibility;
        }

        private void StatusLoading() {
            ImgUhd.Opacity = 0;
            ImgUhd.Scale = new Vector3(1.014f, 1.014f, 1.014f);
            ProgressLoading.ShowPaused = false;
            ProgressLoading.ShowError = false;
            ProgressLoading.Visibility = Visibility.Visible;
            ToggleInfo(null);
        }

        private void StatusError() {
            ImgUhd.Opacity = 0;
            TextTitle.Text = resLoader.GetStringForUri(new Uri("ms-resource:///Resources/TextTItle/Text"));
            TextDetailCaption.Text = "";
            TextDetailLocation.Text = "";
            TextDetailDesc.Text = "";
            TextDetailCopyright.Text = "";
            TextDetailDate.Text = "";
            TextDetailProperties.Text = "";
            ProgressLoading.ShowError = true;
            ProgressLoading.Visibility = Visibility.Visible;

            BtnSetDesktop.IsEnabled = false;
            BtnSetLock.IsEnabled = false;
            BtnVolumnOn.Visibility = Visibility.Collapsed;
            BtnVolumnOff.Visibility = Visibility.Collapsed;
            BtnSave.IsEnabled = false;

            ToggleInfo(!NetworkInterface.GetIsNetworkAvailable() ? resLoader.GetString("MsgNoInternet")
                : string.Format(resLoader.GetString("MsgLostProvider"), resLoader.GetString("Provider_" + provider.Id)));
        }

        private async void DownloadAsync() {
            bool res = await BaseProvider.Download(meta, resLoader.GetString("AppNameShort"),
                resLoader.GetString("Provider_" + provider.Id));
            if (res) {
                ToggleInfo(resLoader.GetString("MsgSave1"), InfoBarSeverity.Success, true);
            } else {
                ToggleInfo(resLoader.GetString("MsgSave0"));
            }
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

        private async void LaunchIni() {
            try {
                _ = await Launcher.LaunchFileAsync(await IniUtil.GetIniPath());
            } catch (Exception) {
                Debug.WriteLine("launch file failed");
            }
        }

        private async void LaunchPicLib() {
            try {
                var folder = await KnownFolders.PicturesLibrary.GetFolderAsync(resLoader.GetString("AppNameShort"));
                _ = await Launcher.LaunchFolderAsync(folder);
            } catch (Exception) {
                Debug.WriteLine("launch folder failed");
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
                TextDetailDesc.Visibility = TextDetailDesc.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                TextDetailCopyright.Visibility = TextDetailCopyright.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                TextDetailDate.Visibility = TextDetailDate.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                TextDetailProperties.Visibility = TextDetailProperties.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            } else {
                TextDetailCaption.Visibility = Visibility.Collapsed;
                TextDetailLocation.Visibility = Visibility.Collapsed;
                TextDetailDesc.Visibility = Visibility.Collapsed;
                TextDetailCopyright.Visibility = Visibility.Collapsed;
                TextDetailDate.Visibility = Visibility.Collapsed;
                TextDetailProperties.Visibility = Visibility.Collapsed;
            }
        }

        private void ToggleInfo(string msg, InfoBarSeverity severity = InfoBarSeverity.Error, bool linkLibPic = false) {
            if (string.IsNullOrEmpty(msg)) {
                Info.IsOpen = false;
                return;
            }
            Info.Severity = severity;
            Info.Message = msg;
            BtnLibPic.Visibility = linkLibPic ? Visibility.Visible : Visibility.Collapsed;
            Info.IsOpen = true;
        }

        //private void UpdateTile(Meta meta) {
        //    if (meta?.Thumb == null) {
        //        return;
        //    }
        //    string title;
        //    string subtitle;
        //    if (!string.IsNullOrEmpty(meta.Title)) {
        //        title = meta.Title;
        //        subtitle = meta.Caption ?? "";
        //    } else {
        //        title = meta.Caption;
        //        subtitle = meta.Location ?? "";
        //    }
        //    string content = $@"<tile><visual>
        //        <binding template='TileMedium'>
        //            <text>{title}</text>
        //        </binding>
        //        <binding template='TileWide'>
        //            <text hint-style='subtitle'>{title}</text>
        //            <text hint-style='captionSubtle'>{subtitle}</text>
        //        </binding>
        //        <binding template='TileLarge'>
        //            <text>{meta.Copyright}</text>
        //            <text hint-style='subtitle'>{title}</text>
        //            <text hint-style='captionSubtle'>{subtitle}</text>
        //        </binding>
        //    </visual></tile>";
        //    XmlDocument doc = new XmlDocument();
        //    doc.LoadXml(content);
        //    var notification = new TileNotification(doc);
        //    TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        //    Debug.WriteLine("push");
        //}

        //private void UpdateBadgeGlyph() {
        //    string badgeGlyphValue = "activity";

        //    // Get the blank badge XML payload for a badge glyph
        //    XmlDocument badgeXml =
        //        BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);

        //    // Set the value of the badge in the XML to our glyph value
        //    XmlElement badgeElement =
        //        badgeXml.SelectSingleNode("/badge") as XmlElement;
        //    badgeElement.SetAttribute("value", badgeGlyphValue);

        //    // Create the badge notification
        //    BadgeNotification badge = new BadgeNotification(badgeXml);

        //    // Create the badge updater for the application
        //    BadgeUpdater badgeUpdater =
        //        BadgeUpdateManager.CreateBadgeUpdaterForApplication();

        //    // And update the badge
        //    badgeUpdater.Update(badge);
        //}

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
    }
}
