using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace TimelineWallpaper {
    public sealed partial class DonateDlg : ContentDialog {
        private bool doNotClose = false;

        public DonateDlg() {
            this.InitializeComponent();

            ChangeCode();
        }

        private void ChangeCode(bool viaAlipay = false) {
            ImgDonate.Source = new BitmapImage(new Uri(viaAlipay ? "ms-appx:///Assets/Images/donate_alipay.png" : "ms-appx:///Assets/Images/donate_wechat.png"));
        }

        private void Donate_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            ChangeCode();
            doNotClose = true;
        }

        private void Donate_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            ChangeCode(true);
            doNotClose = true;
        }

        private void Donate_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            doNotClose = false;
        }

        private void Donate_Closing(ContentDialog sender, ContentDialogClosingEventArgs args) {
            args.Cancel = doNotClose;
        }
    }
}
