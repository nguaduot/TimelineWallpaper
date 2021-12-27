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
        private Ini ini = new Ini();

        private readonly ResourceLoader resLoader;

        private readonly List<Paras> listBingLang = new List<Paras>();
        private readonly List<Paras> listOneplusOrder = new List<Paras>();
        private readonly List<Paras> listTimelineCate = new List<Paras>();
        private readonly List<Paras> listTimelineOrder = new List<Paras>();
        private readonly List<Paras> listYmyouliCol = new List<Paras>();
        private readonly List<Paras> listInfinityOrder = new List<Paras>();

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
            foreach (string item in YmyouliIni.COL_MODULE_DIC.Keys) {
                listYmyouliCol.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("YmyouliCol_" + item)
                });
            }
            foreach (string item in InfinityIni.ORDER) {
                listInfinityOrder.Add(new Paras {
                    Id = item,
                    Name = resLoader.GetString("InfinityOrder_" + item)
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

        public async void PaneOpenedOrClosed(Ini ini, bool opened) {
            if (!opened) {
                return;
            }
            this.ini = ini;

            ExpanderBing.IsExpanded = BingIni.ID.Equals(ini.Provider);
            ExpanderNasa.IsExpanded = NasaIni.ID.Equals(ini.Provider);
            ExpanderOneplus.IsExpanded = OneplusIni.ID.Equals(ini.Provider);
            ExpanderTimeline.IsExpanded = TimelineIni.ID.Equals(ini.Provider);
            ExpanderHimawari8.IsExpanded = Himawari8Ini.ID.Equals(ini.Provider);
            ExpanderYmyouli.IsExpanded = YmyouliIni.ID.Equals(ini.Provider);
            ExpanderInfinity.IsExpanded = InfinityIni.ID.Equals(ini.Provider);

            BoxBingLang.SelectedIndex = BingIni.LANG.IndexOf(((BingIni)ini.GetIni(BingIni.ID)).Lang);
            ToggleNasaMirror.IsOn = "bjp".Equals(((NasaIni)ini.GetIni(NasaIni.ID)).Mirror);
            BoxOneplusOrder.SelectedIndex = OneplusIni.ORDER.IndexOf(((OneplusIni)ini.GetIni(OneplusIni.ID)).Order);
            BoxTimelineCate.SelectedIndex = TimelineIni.CATE.IndexOf(((TimelineIni)ini.GetIni(TimelineIni.ID)).Cate);
            BoxTimelineOrder.SelectedIndex = TimelineIni.ORDER.IndexOf(((TimelineIni)ini.GetIni(TimelineIni.ID)).Order);
            BoxHimawari8Offset.Value = ((Himawari8Ini)ini.GetIni(Himawari8Ini.ID)).Offset;
            BoxInfinityOrder.SelectedIndex = InfinityIni.ORDER.IndexOf(((InfinityIni)ini.GetIni(InfinityIni.ID)).Order);
            BoxYmyouliCol.SelectedIndex = Enumerable.ToList(YmyouliIni.COL_MODULE_DIC.Keys).IndexOf(((YmyouliIni)ini.GetIni(YmyouliIni.ID)).Col);

            RadioButton rb = RbTheme.Items.Cast<RadioButton>().FirstOrDefault(c => ini.Theme.Equals(c?.Tag?.ToString()));
            rb.IsChecked = true;
            TextThemeCur.Text = rb.Content.ToString();

            StorageFolder folderSave = await GetFolderSave();
            SettingsSaveDesc.Text = string.Format(resLoader.GetString("DetailSave"), (await folderSave.GetFilesAsync()).Count);
        }

        private void RbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is RadioButton selectItem) {
                TextThemeCur.Text = selectItem.Content.ToString();
                ini.Theme = selectItem.Tag?.ToString();
                IniUtil.SaveTheme(ini.Theme);
                if (Window.Current.Content is FrameworkElement rootElement) {
                    switch (ini.Theme) {
                        case "light":
                            rootElement.RequestedTheme = ElementTheme.Light;
                            break;
                        case "dark":
                            rootElement.RequestedTheme = ElementTheme.Dark;
                            break;
                        default: // TODO：bug
                            rootElement.RequestedTheme = ElementTheme.Default;
                            break;
                    }
                }
            }
        }

        private void LinkDonate_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args) {
            _ = new DonateDlg().ShowAsync();
        }

        private async void BtnIni_Click(object sender, RoutedEventArgs e) {
            LaunchFile(await IniUtil.GetIniPath());
        }

        private async void BtnShowIni_Click(object sender, RoutedEventArgs e) {
            StorageFile file = await IniUtil.GetIniPath();
            LaunchFolder(await file.GetParentAsync(), file);
        }

        private async void BtnShowSave_Click(object sender, RoutedEventArgs e) {
            LaunchFolder(await GetFolderSave());
        }

        private void BtnShowCache_Click(object sender, RoutedEventArgs e) {
            LaunchFolder(ApplicationData.Current.TemporaryFolder);
        }

        private void BoxBingLang_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Paras paras = e.AddedItems[0] as Paras;
            ((BingIni)ini.GetIni(BingIni.ID)).Lang = paras.Id;
            IniUtil.SaveBingLang(paras.Id);
        }

        private void ToggleNasaMirror_Toggled(object sender, RoutedEventArgs e) {
            string mirror = ((ToggleSwitch)sender).IsOn ? "bjp" : "";
            ((NasaIni)ini.GetIni(NasaIni.ID)).Mirror = mirror;
            IniUtil.SaveNasaMirror(mirror);
        }

        private void BoxOneplusOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Paras paras = e.AddedItems[0] as Paras;
            ((OneplusIni)ini.GetIni(OneplusIni.ID)).Order = paras.Id;
            IniUtil.SaveOneplusOrder(paras.Id);
        }

        private void BoxTimelineCate_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Paras paras = e.AddedItems[0] as Paras;
            ((TimelineIni)ini.GetIni(TimelineIni.ID)).Cate = paras.Id;
            IniUtil.SaveTimelineCate(paras.Id);
        }

        private void BoxTimelineOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Paras paras = e.AddedItems[0] as Paras;
            ((TimelineIni)ini.GetIni(TimelineIni.ID)).Order = paras.Id;
            IniUtil.SaveTimelineOrder(paras.Id);
        }

        private void BoxHimawari8Offset_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args) {
            ((Himawari8Ini)ini.GetIni(Himawari8Ini.ID)).Offset = (float)args.NewValue;
            IniUtil.SaveHimawari8Offset((float)args.NewValue);
        }

        private void BoxInfinityOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Paras paras = e.AddedItems[0] as Paras;
            ((InfinityIni)ini.GetIni(InfinityIni.ID)).Order = paras.Id;
            IniUtil.SaveInfinityOrder(paras.Id);
        }

        private void BoxYmyouliCol_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Paras paras = e.AddedItems[0] as Paras;
            ((YmyouliIni)ini.GetIni(YmyouliIni.ID)).Col = paras.Id;
            IniUtil.SaveYmyouliCol(paras.Id);
        }
    }

    public class Paras {
        public string Id { get; set; }

        public string Name { get; set; }

        override public string ToString() => Name;
    }
}
