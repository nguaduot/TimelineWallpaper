using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimelineWallpaper.Beans;
using TimelineWallpaper.Services;
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
    public sealed partial class ContributeDlg : ContentDialog {
        public ContributeDlg() {
            this.InitializeComponent();
        }

        public ContributeApiReq GetContent() {
            return new ContributeApiReq {
                Url = BoxUrl.Text.Trim(),
                Title = BoxTitle.Text.Trim(),
                Story = BoxStory.Text.Trim(),
                Contact = BoxContact.Text.Trim()
            };
        }

        private void BoxUrl_TextChanged(object sender, TextChangedEventArgs e) {
            this.IsPrimaryButtonEnabled = BoxUrl.Text.Trim().Length > 0;
        }
    }
}
