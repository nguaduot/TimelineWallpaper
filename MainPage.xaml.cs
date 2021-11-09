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

        private void BtnOneplusSort_Click(object sender, RoutedEventArgs e) {
            IniUtil.SaveOneplusSort(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void Btn3GSort_Click(object sender, RoutedEventArgs e) {
            IniUtil.Save3GSort(((RadioMenuFlyoutItem)sender).Tag.ToString());
            ini = null;
            provider = null;
            StatusLoading();
            LoadFocusAsync();
        }

        private void BtnPixivelSanity_Click(object sender, RoutedEventArgs e) {
            IniUtil.SavePixivelSanity(((RadioMenuFlyoutItem)sender).Tag.ToString());
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

        private void BtnIni_Click(object sender, RoutedEventArgs e) {
            LaunchIni();
        }

        private void ViewBar_PointerEntered(object sender, PointerRoutedEventArgs e) {
            ProgressLoading.Visibility = Visibility.Visible;
            ToggleStory(true);
            Info.IsOpen = false;
        }

        private void ViewBar_PointerExited(object sender, PointerRoutedEventArgs e) {
            if (ProgressLoading.ShowPaused && !ProgressLoading.ShowError) {
                ProgressLoading.Visibility = Visibility.Collapsed;
            }
            ToggleStory(false);
            Info.IsOpen = false;
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

        private void ImgBiOpend(object sender, RoutedEventArgs e) {
            //float ratioImg = (float)bi.PixelWidth / bi.PixelHeight;
            //float ratioWin = Window.Current.Content.ActualSize.X / Window.Current.Content.ActualSize.Y;
            //TipPreview.IsOpen = Math.Abs(ratioImg - ratioWin) / ratioWin > 0.1;
            string imgSize = sender is BitmapImage bi ? bi.PixelWidth + "x" + bi.PixelHeight : "0x0";
            StorageFile file = meta.CacheUhd ?? meta.CacheVideo ?? meta.CacheAudio;
            string fileSize = FileUtil.ConvertFileSize(file != null ? new FileInfo(file.Path).Length : 0);
            TextDetailSize.Text = string.Format("{0} | {1}, {2}",
                resLoader.GetString("Provider_" + provider.Id), imgSize, fileSize);
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
            BtnBingLang.Visibility = BtnProviderBing.IsChecked
                ? Visibility.Visible : Visibility.Collapsed;
            BtnProviderNasa.IsChecked = BtnProviderNasa.Tag.Equals(ini.Provider);
            BtnNasaMirror.Visibility = BtnProviderNasa.IsChecked
                ? Visibility.Visible : Visibility.Collapsed;
            BtnProviderOneplus.IsChecked = BtnProviderOneplus.Tag.Equals(ini.Provider);
            BtnOneplusSort.Visibility = BtnProviderOneplus.IsChecked
                ? Visibility.Visible : Visibility.Collapsed;
            BtnProvider3G.IsChecked = BtnProvider3G.Tag.Equals(ini.Provider);
            Btn3GSort.Visibility = BtnProvider3G.IsChecked
                ? Visibility.Visible : Visibility.Collapsed;
            BtnProviderDaihan.IsChecked = BtnProviderDaihan.Tag.Equals(ini.Provider);
            BtnProviderYmyouli.IsChecked = BtnProviderYmyouli.Tag.Equals(ini.Provider);
            BtnYmyouliCol.Visibility = BtnProviderYmyouli.IsChecked
                ? Visibility.Visible : Visibility.Collapsed;

            BtnBingLangDef.IsChecked = BtnBingLangDef.Tag.Equals(ini.Bing.Lang);
            BtnBingLangCn.IsChecked = BtnBingLangCn.Tag.Equals(ini.Bing.Lang);
            BtnBingLangJp.IsChecked = BtnBingLangJp.Tag.Equals(ini.Bing.Lang);
            BtnBingLangJp.IsChecked = BtnBingLangJp.Tag.Equals(ini.Bing.Lang);
            BtnBingLangDe.IsChecked = BtnBingLangDe.Tag.Equals(ini.Bing.Lang);
            BtnBingLangFr.IsChecked = BtnBingLangFr.Tag.Equals(ini.Bing.Lang);
            BtnNasaMirrorDef.IsChecked = BtnNasaMirrorDef.Tag.Equals(ini.Nasa.Mirror);
            BtnNasaMirrorBjp.IsChecked = BtnNasaMirrorBjp.Tag.Equals(ini.Nasa.Mirror);
            BtnOneplusSort1.IsChecked = BtnOneplusSort1.Tag.Equals(ini.OnePlus.Sort);
            BtnOneplusSort2.IsChecked = BtnOneplusSort2.Tag.Equals(ini.OnePlus.Sort);
            BtnOneplusSort3.IsChecked = BtnOneplusSort3.Tag.Equals(ini.OnePlus.Sort);
            Btn3GSort1.IsChecked = Btn3GSort1.Tag.Equals(ini.G3.Sort);
            Btn3GSort2.IsChecked = Btn3GSort2.Tag.Equals(ini.G3.Sort);
            BtnYmyouliColDef.IsChecked = BtnYmyouliColDef.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol126.IsChecked = BtnYmyouliCol126.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol182.IsChecked = BtnYmyouliCol182.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol183.IsChecked = BtnYmyouliCol183.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol215.IsChecked = BtnYmyouliCol215.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol184.IsChecked = BtnYmyouliCol184.Tag.Equals(ini.Ymyouli.Col);
            BtnYmyouliCol185.IsChecked = BtnYmyouliCol184.Tag.Equals(ini.Ymyouli.Col);
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
        }

        private void ShowImg(Meta meta) {
            if (meta == null) {
                StatusError();
                return;
            }
            if (meta.CacheUhd != null) {
                ImgUhd.Source = null;
                BitmapImage biUhd = new BitmapImage();
                ImgUhd.PosterSource = biUhd;
                //biUhd.DecodePixelType = DecodePixelType.Logical;
                //biUhd.DecodePixelWidth = (int)Window.Current.Content.ActualSize.X;
                //biUhd.DecodePixelHeight = (int)Window.Current.Content.ActualSize.Y;
                biUhd.ImageOpened += ImgBiOpend;
                biUhd.UriSource = new Uri(meta.CacheUhd.Path, UriKind.Absolute);
            } else if (meta.CacheVideo != null) {
                ImgUhd.PosterSource = null;
                ImgUhd.Source = meta.CacheVideo != null ? MediaSource.CreateFromStorageFile(meta.CacheVideo) : null;
                //MediaClip mediaClip = await MediaClip.CreateFromFileAsync(meta.CacheVideo);
                //MediaComposition mediaComposition = new MediaComposition();
                //mediaComposition.Clips.Add(mediaClip);
                //BitmapImage bi = new BitmapImage();
                //ImgPreview.Source = bi;
                //bi.ImageOpened += ImgBiOpend;
                //bi.SetSource(await mediaComposition.GetThumbnailAsync(
                //    TimeSpan.FromMilliseconds(5000), 0, 0, VideoFramePrecision.NearestFrame));
            } else {
                ImgUhd.Source = null;
                ImgUhd.PosterSource = null;
            }

            BtnSetDesktop.Visibility = meta.CacheUhd != null ? Visibility.Visible : Visibility.Collapsed;
            BtnSetLock.Visibility = meta.CacheUhd != null ? Visibility.Visible : Visibility.Collapsed;
            BtnVolumnOn.Visibility = (meta.CacheVideo != null || meta.CacheAudio != null) && ImgUhd.MediaPlayer.Volume == 0
                ? Visibility.Visible : Visibility.Collapsed;
            BtnVolumnOff.Visibility = (meta.CacheVideo != null || meta.CacheAudio != null) && ImgUhd.MediaPlayer.Volume > 0
                ? Visibility.Visible : Visibility.Collapsed;
            BtnSave.IsEnabled = meta.CacheUhd != null || meta.CacheVideo != null || meta.CacheAudio != null;

            //SolidColorBrush brush;
            //if (meta.Dominant != null) {
            //    brush = meta.Dominant;
            //} else {
            //    UISettings uiSettings = new UISettings();
            //    brush = new SolidColorBrush(uiSettings.GetColorValue(UIColorType.Accent));
            //}
            //ColorBar1.Fill = brush;
            //ColorBar2.Fill = brush;

            StatusEnjoy();
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
            Info.IsOpen = false;
        }

        private void StatusError() {
            ImgUhd.Opacity = 0;
            TextTitle.Text = resLoader.GetStringForUri(new Uri("ms-resource:///Resources/TextTItle/Text"));
            TextDetailCaption.Text = "";
            TextDetailLocation.Text = "";
            TextDetailDesc.Text = "";
            TextDetailCopyright.Text = "";
            TextDetailDate.Text = "";
            ProgressLoading.ShowError = true;
            ProgressLoading.Visibility = Visibility.Visible;
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                Info.Message = resLoader.GetString("MsgNoInternet");
            } else {
                Info.Message = resLoader.GetString("MsgLostProvider");
            }
            Info.Severity = InfoBarSeverity.Error;
            Info.Title = resLoader.GetString("TitleErrLoad");
            Info.IsOpen = true;
        }

        private async void DownloadAsync() {
            if (await BaseProvider.Download(provider, meta)) {
                Info.Severity = InfoBarSeverity.Success;
                Info.Message = resLoader.GetString("MsgSave1");
            } else {
                Info.Severity = InfoBarSeverity.Error;
                Info.Message = resLoader.GetString("MsgSave0");
            }
            Info.Title = resLoader.GetStringForUri(new Uri("ms-resource:///Resources/BtnSave/Text"));
            Info.IsOpen = true;
        }

        private async void SetWallpaperAsync(Meta meta, bool setDesktopOrLock) {
            if (meta?.CacheUhd == null) {
                return;
            }

            string title = resLoader.GetStringForUri(new Uri(setDesktopOrLock
                ? "ms-resource:///Resources/BtnSetDesktop/Text"
                : "ms-resource:///Resources/BtnSetLock/Text"));
            if (!UserProfilePersonalizationSettings.IsSupported()) {
                Info.Severity = InfoBarSeverity.Error;
                Info.Message = resLoader.GetString("MsgWallpaper0");
                Info.Title = title;
                Info.IsOpen = true;
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
                Info.Severity = InfoBarSeverity.Success;
                Info.Message = resLoader.GetString(setDesktopOrLock ? "MsgSetDesktop1" : "MsgSetLock1");
            } else {
                Info.Severity = InfoBarSeverity.Error;
                Info.Message = resLoader.GetString(setDesktopOrLock ? "MsgSetDesktop0" : "MsgSetLock0");
            }
            Info.Title = title;
            Info.IsOpen = true;
        }

        private async void LaunchIni() {
            _ = await Launcher.LaunchFileAsync(await IniUtil.GetIniPath());
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
                TextDetailSize.Visibility = TextDetailSize.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            } else {
                TextDetailCaption.Visibility = Visibility.Collapsed;
                TextDetailLocation.Visibility = Visibility.Collapsed;
                TextDetailDesc.Visibility = Visibility.Collapsed;
                TextDetailCopyright.Visibility = Visibility.Collapsed;
                TextDetailDate.Visibility = Visibility.Collapsed;
                TextDetailSize.Visibility = Visibility.Collapsed;
            }
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
                Info.Severity = InfoBarSeverity.Error;
                Info.Message = resLoader.GetString("TitleErrPush");
                Info.Title = resLoader.GetString("MsgNoPermission");
                Info.IsOpen = true;
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
