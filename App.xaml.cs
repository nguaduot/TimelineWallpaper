using System;
using System.Threading.Tasks;
using TimelineWallpaper.Services;
using TimelineWallpaper.Utils;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TimelineWallpaper {
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App() {
            ChangeTheme();

            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // 集成崩溃日志反馈
            //AppCenter.Start("867dbb71-eaa5-4525-8f70-9877a65d0796", typeof(Crashes));
            // 上传崩溃日志
            this.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedException;
            AppDomain.CurrentDomain.UnhandledException += OnBgUnhandledException;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null) {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false) {
                if (rootFrame.Content == null) {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                // 确保当前窗口处于活动状态
                Window.Current.Activate();

                // 首次启动设置最佳窗口比例：16:10
                OptimizeSize();
                // 将内容扩展到标题栏，并使标题栏半透明
                TransTitleBar();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e) {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        private void ChangeTheme() {
            // b注意：ElementTheme.Default 指向该值，非系统主题
            switch (IniUtil.GetIni().Theme) {
                case "light":
                    this.RequestedTheme = ApplicationTheme.Light;
                    break;
                case "dark":
                    this.RequestedTheme = ApplicationTheme.Dark;
                    break;
            }
        }

        private void OptimizeSize() {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("optimizeSize")) {
                ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(960, 600);
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
                ApplicationData.Current.LocalSettings.Values["optimizeSize"] = true;
            }
        }

        private void TransTitleBar() {
            // https://docs.microsoft.com/zh-cn/windows/apps/design/style/acrylic#extend-acrylic-into-the-title-bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e) {
            ApiService.Crash(e.Exception);
            e.Handled = true;
            // TODO
            //_ = await new ContentDialog {
            //    Title = "未知异常1",
            //    Content = e.Exception.Message,
            //    CloseButtonText = "关闭",
            //    DefaultButton = ContentDialogButton.Close
            //}.ShowAsync();
        }

        private void OnUnobservedException(object sender, UnobservedTaskExceptionEventArgs e) {
            ApiService.Crash(e.Exception);
            e.SetObserved();
            // TODO
            //_ = await new ContentDialog {
            //    Title = "未知异常2",
            //    Content = e.Exception,
            //    CloseButtonText = "关闭",
            //    DefaultButton = ContentDialogButton.Close
            //}.ShowAsync();
        }

        private void OnBgUnhandledException(object sender, System.UnhandledExceptionEventArgs e) {
            ApiService.Crash((Exception)e.ExceptionObject);
            // TODO
            //_ = await new ContentDialog {
            //    Title = "未知异常3",
            //    Content = ((Exception)e.ExceptionObject).Message,
            //    CloseButtonText = "关闭",
            //    DefaultButton = ContentDialogButton.Close
            //}.ShowAsync();
        }
    }
}
