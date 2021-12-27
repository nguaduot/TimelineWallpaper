using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimelineWallpaper.Utils;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace TimelineWallpaper {
    public sealed partial class ReviewDlg : ContentDialog {
        public ReviewDlg() {
            this.InitializeComponent();

            this.Title = ResourceLoader.GetForCurrentView().GetString("AppNameShort") + " " + VerUtil.GetPkgVer(true);
        }

        private void LinkDonate_Click(object sender, RoutedEventArgs e) {
            this.Hide();
            DonateDlg dlgDonate = new DonateDlg();
            _ = dlgDonate.ShowAsync();
        }
    }
}
