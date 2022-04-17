using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;
using Windows.ApplicationModel.Resources;
using Windows.Globalization.NumberFormatting;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace TimelineWallpaper {
    public sealed partial class SettingsView : UserControl {
        public event EventHandler<SettingsEventArgs> SettingsChanged;
        public event EventHandler<EventArgs> ContributeChanged;

        private Ini ini = new Ini();

        private readonly ResourceLoader resLoader;

        private readonly List<Paras> listBingLang = new List<Paras>();
        private readonly List<Paras> listOneplusOrder = new List<Paras>();
        private readonly List<Paras> listTimelineCate = new List<Paras>();
        private readonly List<Paras> listTimelineOrder = new List<Paras>();
        private readonly List<Paras> listYmyouliCate = new List<Paras>();
        private readonly List<Paras> listYmyouliOrder = new List<Paras>();
        private readonly List<Paras> listInfinityOrder = new List<Paras>();
        private readonly List<Paras> listOneOrder = new List<Paras>();
        private readonly List<Paras> listQingbzCate = new List<Paras>();
        private readonly List<Paras> listQingbzOrder = new List<Paras>();

        private bool paneOpened = false; // 避免初始化设置选项的非必要事件

        private DispatcherTimer settingsTimer = null;

        public SettingsView() {
            this.InitializeComponent();

            resLoader = ResourceLoader.GetForCurrentView();
            Init();
        }

        private void Init() {
            TextApp.Text = resLoader.GetString("AppName") + " " + VerUtil.GetPkgVer(false);

            foreach (string item in BingIni.LANG) {
                listBingLang.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("BingLang_" + item)
                });
            }
            foreach (string item in OneplusIni.ORDER) {
                listOneplusOrder.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("OneplusOrder_" + item)
                });
            }
            foreach (string item in TimelineIni.CATE) {
                listTimelineCate.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("TimelineCate_" + item)
                });
            }
            foreach (string item in TimelineIni.ORDER) {
                listTimelineOrder.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("TimelineOrder_" + item)
                });
            }
            foreach (string item in YmyouliIni.CATE) {
                listYmyouliCate.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("YmyouliCate_" + item)
                });
            }
            foreach (string item in YmyouliIni.ORDER) {
                listYmyouliOrder.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("YmyouliOrder_" + item)
                });
            }
            foreach (string item in InfinityIni.ORDER) {
                listInfinityOrder.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("InfinityOrder_" + item)
                });
            }
            foreach (string item in OneIni.ORDER) {
                listOneOrder.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("OneOrder_" + item)
                });
            }
            foreach (string item in QingbzIni.CATE) {
                listQingbzCate.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("QingbzCate_" + item)
                });
            }
            foreach (string item in QingbzIni.ORDER) {
                listQingbzOrder.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("QingbzOrder_" + item)
                });
            }

            BoxHimawari8Offset.NumberFormatter = new DecimalFormatter {
                IntegerDigits = 1,
                FractionDigits = 2,
                NumberRounder = new IncrementNumberRounder {
                    Increment = 0.01,
                    RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp
                }
            };
        }

        private async void LaunchFile(StorageFile file) {
            try {
                _ = await Launcher.LaunchFileAsync(file);
            } catch (Exception) {
                Debug.WriteLine("launch file failed");
            }
        }

        private async Task<StorageFolder> GetFolderSave() {
            return await KnownFolders.PicturesLibrary.CreateFolderAsync(resLoader.GetString("AppNameShort"),
                CreationCollisionOption.OpenIfExists);
        }

        private async void LaunchFolder(StorageFolder folder, StorageFile fileSelected = null) {
            try {
                if (fileSelected != null) {
                    FolderLauncherOptions options = new FolderLauncherOptions();
                    options.ItemsToSelect.Add(fileSelected); // 打开文件夹同时选中目标文件
                    _ = await Launcher.LaunchFolderAsync(folder, options);
                } else {
                    _ = await Launcher.LaunchFolderAsync(folder);
                }
            } catch (Exception) {
                Debug.WriteLine("launch folder failed");
            }
        }

        public void BeforePaneOpen(Ini ini) {
            this.ini = ini;
            paneOpened = false;

            ExpanderBing.IsExpanded = BingIni.ID.Equals(ini.Provider);
            ExpanderNasa.IsExpanded = NasaIni.ID.Equals(ini.Provider);
            ExpanderOneplus.IsExpanded = OneplusIni.ID.Equals(ini.Provider);
            ExpanderTimeline.IsExpanded = TimelineIni.ID.Equals(ini.Provider);
            ExpanderHimawari8.IsExpanded = Himawari8Ini.ID.Equals(ini.Provider);
            ExpanderYmyouli.IsExpanded = YmyouliIni.ID.Equals(ini.Provider);
            ExpanderInfinity.IsExpanded = InfinityIni.ID.Equals(ini.Provider);
            ExpanderOne.IsExpanded = OneIni.ID.Equals(ini.Provider);
            ExpanderQingbz.IsExpanded = QingbzIni.ID.Equals(ini.Provider);

            BoxBingLang.SelectedIndex = listBingLang.Select(t => t.Id).ToList().IndexOf(((BingIni)ini.GetIni(BingIni.ID)).Lang);
            ToggleNasaMirror.IsOn = "bjp".Equals(((NasaIni)ini.GetIni(NasaIni.ID)).Mirror);
            BoxOneplusOrder.SelectedIndex = listOneplusOrder.Select(t => t.Id).ToList().IndexOf(((OneplusIni)ini.GetIni(OneplusIni.ID)).Order);
            BoxTimelineCate.SelectedIndex = listTimelineCate.Select(t => t.Id).ToList().IndexOf(((TimelineIni)ini.GetIni(TimelineIni.ID)).Cate);
            BoxTimelineOrder.SelectedIndex = listTimelineOrder.Select(t => t.Id).ToList().IndexOf(((TimelineIni)ini.GetIni(TimelineIni.ID)).Order);
            BoxHimawari8Offset.Value = ((Himawari8Ini)ini.GetIni(Himawari8Ini.ID)).Offset;
            BoxYmyouliCate.SelectedIndex = listYmyouliCate.Select(t => t.Id).ToList().IndexOf(((YmyouliIni)ini.GetIni(YmyouliIni.ID)).Cate);
            BoxYmyouliOrder.SelectedIndex = listYmyouliOrder.Select(t => t.Id).ToList().IndexOf(((YmyouliIni)ini.GetIni(YmyouliIni.ID)).Order);
            BoxInfinityOrder.SelectedIndex = listInfinityOrder.Select(t => t.Id).ToList().IndexOf(((InfinityIni)ini.GetIni(InfinityIni.ID)).Order);
            BoxOneOrder.SelectedIndex = listOneOrder.Select(t => t.Id).ToList().IndexOf(((OneIni)ini.GetIni(OneIni.ID)).Order);
            BoxQingbzCate.SelectedIndex = listQingbzCate.Select(t => t.Id).ToList().IndexOf(((QingbzIni)ini.GetIni(QingbzIni.ID)).Cate);
            BoxQingbzOrder.SelectedIndex = listQingbzOrder.Select(t => t.Id).ToList().IndexOf(((QingbzIni)ini.GetIni(QingbzIni.ID)).Order);

            RadioButton rb = RbTheme.Items.Cast<RadioButton>().FirstOrDefault(c => ini.Theme.Equals(c?.Tag?.ToString()));
            rb.IsChecked = true;
            TextThemeCur.Text = rb.Content.ToString();

            paneOpened = true;
        }

        private void ExpanderProvider_Expanding(Microsoft.UI.Xaml.Controls.Expander sender, Microsoft.UI.Xaml.Controls.ExpanderExpandingEventArgs args) {
            if (!ini.Provider.Equals(sender.Tag)) {
                IniUtil.SaveProvider(sender.Tag.ToString());
                SettingsChanged?.Invoke(this, new SettingsEventArgs {
                    ProviderChanged = true
                });
            }
        }

        private void RbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is RadioButton selectItem) {
                TextThemeCur.Text = selectItem.Content.ToString();
                string theme = selectItem.Tag?.ToString();
                if (!ini.Theme.Equals(theme)) {
                    if (Window.Current.Content is FrameworkElement rootElement) {
                        rootElement.RequestedTheme = ThemeUtil.ParseTheme(theme);
                    }
                    ini.Theme = theme;
                    IniUtil.SaveTheme(theme);
                    SettingsChanged?.Invoke(this, new SettingsEventArgs {
                        ThemeChanged = true
                    });
                }
            }
        }

        private void LinkDonate_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args) {
            _ = new DonateDlg {
                RequestedTheme = ThemeUtil.ParseTheme(ini.Theme) // 修复未响应主题切换的BUG
            }.ShowAsync();
        }

        private async void BtnIni_Click(object sender, RoutedEventArgs e) {
            LaunchFile(await IniUtil.GetIniPath());
        }

        private async void BtnShowSave_Click(object sender, RoutedEventArgs e) {
            LaunchFolder(await GetFolderSave());
        }

        private void BtnShowCache_Click(object sender, RoutedEventArgs e) {
            LaunchFolder(ApplicationData.Current.TemporaryFolder);
        }

        private void BoxBingLang_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            Paras paras = e.AddedItems[0] as Paras;
            BingIni bi = (BingIni)ini.GetIni(BingIni.ID);
            bi.Lang = paras.Id;
            IniUtil.SaveBingLang(paras.Id);
            IniUtil.SaveProvider(BingIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void ToggleNasaMirror_Toggled(object sender, RoutedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            string mirror = ((ToggleSwitch)sender).IsOn ? "bjp" : "";
            NasaIni bi = (NasaIni)ini.GetIni(NasaIni.ID);
            bi.Mirror = mirror;
            IniUtil.SaveNasaMirror(mirror);
            IniUtil.SaveProvider(NasaIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void BoxOneplusOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            Paras paras = e.AddedItems[0] as Paras;
            OneplusIni bi = (OneplusIni)ini.GetIni(OneplusIni.ID);
            bi.Order = paras.Id;
            IniUtil.SaveOneplusOrder(paras.Id);
            IniUtil.SaveProvider(OneplusIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void BoxTimelineCate_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            Paras paras = e.AddedItems[0] as Paras;
            TimelineIni bi = (TimelineIni)ini.GetIni(TimelineIni.ID);
            bi.Cate = paras.Id;
            IniUtil.SaveTimelineCate(paras.Id);
            IniUtil.SaveProvider(TimelineIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void BoxTimelineOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            Paras paras = e.AddedItems[0] as Paras;
            TimelineIni bi = (TimelineIni)ini.GetIni(TimelineIni.ID);
            bi.Order = paras.Id;
            IniUtil.SaveTimelineOrder(paras.Id);
            IniUtil.SaveProvider(TimelineIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void BtnTimelineContribute_Click(object sender, RoutedEventArgs e) {
            ContributeChanged?.Invoke(this, new EventArgs());
        }

        private void BoxHimawari8Offset_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args) {
            if (!paneOpened) {
                return;
            }
            if (settingsTimer == null) {
                settingsTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1000) };
                settingsTimer.Tick += (sender2, e2) => {
                    settingsTimer.Stop();
                    float offset = (float)BoxHimawari8Offset.Value;
                    Himawari8Ini bi = (Himawari8Ini)ini.GetIni(Himawari8Ini.ID);
                    if (Math.Abs(offset - bi.Offset) < 0.01f) {
                        return;
                    }
                    bi.Offset = offset;
                    IniUtil.SaveHimawari8Offset(offset);
                    IniUtil.SaveProvider(Himawari8Ini.ID);
                    SettingsChanged?.Invoke(this, new SettingsEventArgs {
                        ProviderChanged = true
                    });
                };
            }
            settingsTimer.Stop();
            settingsTimer.Start();
        }

        private void BoxYmyouliCate_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            Paras paras = e.AddedItems[0] as Paras;
            YmyouliIni bi = (YmyouliIni)ini.GetIni(YmyouliIni.ID);
            bi.Cate = paras.Id;
            IniUtil.SaveYmyouliCate(paras.Id);
            IniUtil.SaveProvider(YmyouliIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void BoxYmyouliOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            Paras paras = e.AddedItems[0] as Paras;
            YmyouliIni bi = (YmyouliIni)ini.GetIni(YmyouliIni.ID);
            bi.Order = paras.Id;
            IniUtil.SaveYmyouliOrder(paras.Id);
            IniUtil.SaveProvider(YmyouliIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void BtnYmyouliDonate_Click(object sender, RoutedEventArgs e) {
            _ = Launcher.LaunchUriAsync(new Uri(resLoader.GetString("UrlYmyouli")));
        }

        private void BoxInfinityOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            Paras paras = e.AddedItems[0] as Paras;
            InfinityIni bi = (InfinityIni)ini.GetIni(InfinityIni.ID);
            bi.Order = paras.Id;
            IniUtil.SaveInfinityOrder(paras.Id);
            IniUtil.SaveProvider(InfinityIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void BoxOneOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            Paras paras = e.AddedItems[0] as Paras;
            OneIni bi = (OneIni)ini.GetIni(OneIni.ID);
            bi.Order = paras.Id;
            IniUtil.SaveOneOrder(paras.Id);
            IniUtil.SaveProvider(OneIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void BoxQingbzCate_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            Paras paras = e.AddedItems[0] as Paras;
            QingbzIni bi = (QingbzIni)ini.GetIni(QingbzIni.ID);
            bi.Cate = paras.Id;
            IniUtil.SaveQingbzCate(paras.Id);
            IniUtil.SaveProvider(QingbzIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void BoxQingbzOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!paneOpened) {
                return;
            }
            Paras paras = e.AddedItems[0] as Paras;
            QingbzIni bi = (QingbzIni)ini.GetIni(QingbzIni.ID);
            bi.Order = paras.Id;
            IniUtil.SaveQingbzOrder(paras.Id);
            IniUtil.SaveProvider(QingbzIni.ID);
            SettingsChanged?.Invoke(this, new SettingsEventArgs {
                ProviderChanged = true
            });
        }

        private void BtnQingbzDonate_Click(object sender, RoutedEventArgs e) {
            _ = Launcher.LaunchUriAsync(new Uri(resLoader.GetString("UrlQingbz")));
        }

        private void BtnReview_Click(object sender, RoutedEventArgs e) {
            _ = Launcher.LaunchUriAsync(new Uri(resLoader.GetStringForUri(new Uri("ms-resource:///Resources/LinkReview/NavigateUri"))));
        }
    }

    public class Paras {
        public string Id { get; set; }

        public string Name { get; set; }

        override public string ToString() => Name;
    }

    public class SettingsEventArgs : EventArgs {
        public bool ProviderChanged { get; set; }

        public bool ThemeChanged { get; set; }
    }
}
