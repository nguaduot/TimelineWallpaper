using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Xaml;

namespace TimelineWallpaper.Utils {
    public class IniUtil {
        // TODO: 参数有变动时需调整配置名
        private const string FILE_INI = "timelinewallpaper-4.0.ini";

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defValue,
            StringBuilder returnedString, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string section, string key, string value, string filePath);

        private static async Task<StorageFile> GenerateIniFileAsync() {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile iniFile = await folder.TryGetItemAsync(FILE_INI) as StorageFile;
            if (iniFile == null) {
                iniFile = await folder.CreateFileAsync(FILE_INI, CreationCollisionOption.ReplaceExisting);
                string version = string.Format("; 拾光 v{0}.{1}",
                    Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor);
                await FileIO.WriteLinesAsync(iniFile, new string[] {
                    version,
                    "; https://github.com/nguaduot/TimelineWallpaper",
                    "; nguaduot@163.com",
                    "",
                    "[timelinewallpaper]",
                    "",
                    "provider=bing",
                    "; provider=bing       图源：Microsoft Bing - 每天发现一个新地方 https://cn.bing.com",
                    "; provider=nasa       图源：NASA - 每日天文一图 https://apod.nasa.gov/apod",
                    "; provider=oneplus    图源：OnePlus - Shot on OnePlus https://photos.oneplus.com",
                    "; provider=timeline   图源：拾光 - 时光如歌，岁月如诗 https://api.nguaduot.cn/timeline",
                    "; provider=himawari8  图源：向日葵8号 - 实时地球 https://himawari8.nict.go.jp",
                    "; provider=ymyouli    图源：一梦幽黎 - 本站资源准备历时数年 https://www.ymyouli.com",
                    "; provider=infinity   图源：Infinity - 精选壁纸 http://cn.infinitynewtab.com",
                    "; provider=one        图源：ONE · 一个 - 复杂世界里，一个就够了 http://m.wufazhuce.com/one",
                    "; provider=3g         图源：3G壁纸 - 电脑壁纸专家 https://desk.3gbizhi.com",
                    "; provider=bobo       图源：BoBoPic - 每天都有好看的壁纸图片 https://bobopic.com",
                    "; provider=lofter     图源：Lofter - 看见每一种兴趣 https://www.lofter.com",
                    "; provider=abyss      图源：Wallpaper Abyss - 壁纸聚集地 https://wall.alphacoders.com",
                    "; provider=daihan     图源：呆憨API - 随机二次元ACG图片 https://api.daihan.top/html/acg.html",
                    "; provider=dmoe       图源：樱花API - 随机二次元图片 https://www.dmoe.cc",
                    "; provider=toubiec    图源：晓晴API - 随机二次元图片 https://acg.toubiec.cn",
                    "; provider=mty        图源：墨天逸API - 随机图片 https://api.mtyqx.cn",
                    "; provider=seovx      图源：夏沫博客API - 在线古风美图二次元 https://cdn.seovx.com",
                    "; provider=paul       图源：保罗API - 随机动漫壁纸 https://api.paugram.com/help/wallpaper",
                    "",
                    "desktopprovider=",
                    "; desktopprovider={provider}  桌面背景推送图源：参数参考 provider（置空则关闭推送）",
                    "",
                    "lockprovider=",
                    "; lockprovider={provider}  锁屏背景推送图源：参数参考 provider（置空则关闭推送）",
                    "",
                    "theme=",
                    "; theme=       主题：跟随系统（默认）",
                    "; theme=light  主题：亮色",
                    "; theme=dark   主题：暗色",
                    "",
                    "[bing]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lang=",
                    "; lang=       语言代码：自动识别（默认）",
                    "; lang=zh-cn  语言代码：中文",
                    "; lang=en-us  语言代码：英文",
                    "; lang=ja-jp  语言代码：日语",
                    "; lang=de-de  语言代码：德语",
                    "; lang=fr-fr  语言代码：法国",
                    "",
                    "[nasa]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "mirror=bjp",
                    "; mirror=     镜像：官方",
                    "; mirror=bjp  镜像：北京天文馆（默认） http://www.bjp.org.cn/mryt",
                    "",
                    "[oneplus]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "order=date",
                    "; order=date  排序：最新添加（默认）",
                    "; order=rate  排序：点赞最多",
                    "; order=view  排序：浏览最多",
                    "",
                    "[timeline]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "order=date",
                    "; order=date    排序：日期（默认）",
                    "; order=random  排序：随机",
                    "",
                    "cate=",
                    "; cate=           类别：全部（默认）",
                    "; cate=landscape  类别：风光摄影",
                    "; cate=portrait   类别：人像摄影",
                    "; cate=culture    类别：人文摄影",
                    "; cate=term       类别：节气摄影",
                    "",
                    "authorize=1",
                    "; authorize={n}  授权：0或1（默认为1，仅展示已授权图片，若手动修改为0，请勿擅自商用未授权图片）",
                    "",
                    "[himawari8]",
                    "",
                    "desktopperiod=1",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为1h/次，开启推送后生效）",
                    "",
                    "lockperiod=2",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为2h/次，开启推送后生效）",
                    "",
                    "offset=0",
                    "; offset={n}  地球位置：-1.0~1.0（默认为0，居中，-1.0~0偏左，0~1.0偏右）",
                    "",
                    "[ymyouli]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "order=random",
                    "; order=date    排序：收录",
                    "; order=score   排序：热度",
                    "; order=random  排序：随机（默认）",
                    "",
                    "cate=",
                    "; cate=              类别：全部（默认）",
                    "; cate=acgcharacter  类别：动漫人物",
                    "; cate=acgscene      类别：动漫场景",
                    "; cate=sky           类别：日暮云天",
                    "; cate=war           类别：战场战争",
                    "; cate=sword         类别：刀光剑影",
                    "; cate=artistry      类别：意境",
                    "; cate=car           类别：机车",
                    "; cate=portrait      类别：人像",
                    "; cate=animal        类别：动物",
                    "; cate=delicacy      类别：美食蔬果",
                    "; cate=nature        类别：山水花草",
                    "",
                    "qc=1",
                    "; qc={n}  质检：0或1（默认为1，仅展示已质检图片，过滤R18内容、含水印图）",
                    "",
                    "[infinity]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "order=",
                    "; order=      排序：随机（默认）",
                    "; order=rate  排序：热度",
                    "",
                    "[one]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "order=date",
                    "; order=date    排序：日期（默认）",
                    "; order=random  排序：随机",
                    "",
                    "[3g]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "order=date",
                    "; order=date  排序：最新壁纸（默认）",
                    "; order=view  排序：热门壁纸",
                    "",
                    "[bobo]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "[lofter]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "[abyss]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "[daihan]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "[dmoe]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "[toubiec]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "[seovx]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "cate=d",
                    "; cate=    类别：美图",
                    "; cate=d   类别：二次元（默认）",
                    "; cate=ha  类别：古风",
                    "",
                    "[paul]",
                    "",
                    "desktopperiod=24",
                    "; desktopperiod={n}  桌面背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    "",
                    "lockperiod=24",
                    "; lockperiod={n}  锁屏背景推送周期：1~24（默认为24h/次，开启推送后生效）",
                    ""
                });
                //StorageFile defFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/timelinewallpaper.ini"));
                //iniFile = await defFile.CopyAsync(ApplicationData.Current.LocalFolder,
                //    iniFileName, NameCollisionOption.ReplaceExisting);
                Debug.WriteLine("generate ini: " + iniFile.Path);
            }
            return iniFile;
        }

        private static string GetIniFile() {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            string iniFile = Path.Combine(folder.Path, FILE_INI);
            return File.Exists(iniFile) ? iniFile : null;
        }

        public static async void SaveProvider(string provider) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("timelinewallpaper", "provider", provider, iniFile.Path);
        }

        //public static async void SavePush(string push) {
        //    StorageFile iniFile = await GenerateIniFileAsync();
        //    _ = WritePrivateProfileString("timelinewallpaper", "push", push, iniFile.Path);
        //}

        //public static async void SavePushProvider(string provider) {
        //    StorageFile iniFile = await GenerateIniFileAsync();
        //    _ = WritePrivateProfileString("timelinewallpaper", "pushprovider", provider, iniFile.Path);
        //}

        public static async void SaveDesktopProvider(string provider) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("timelinewallpaper", "desktopprovider", provider, iniFile.Path);
        }

        public static async void SaveLockProvider(string provider) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("timelinewallpaper", "lockprovider", provider, iniFile.Path);
        }

        public static async void SaveTheme(string theme) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("timelinewallpaper", "theme", theme, iniFile.Path);
        }

        public static async void SaveBingLang(string langCode) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("bing", "lang", langCode, iniFile.Path);
        }

        public static async void SaveNasaMirror(string mirror) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("nasa", "mirror", mirror, iniFile.Path);
        }

        public static async void SaveOneplusOrder(string order) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("oneplus", "order", order, iniFile.Path);
        }

        public static async void SaveTimelineOrder(string order) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("timeline", "order", order, iniFile.Path);
        }

        public static async void SaveTimelineCate(string order) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("timeline", "cate", order, iniFile.Path);
        }

        public static async void SaveHimawari8Offset(float offset) {
            offset = offset < -1 ? -1 : (offset > 1 ? 1 : offset);
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("himawari8", "offset", offset.ToString("0.00"), iniFile.Path);
        }

        public static async void SaveYmyouliOrder(string order) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("ymyouli", "order", order, iniFile.Path);
        }

        public static async void SaveYmyouliCate(string cate) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("ymyouli", "cate", cate, iniFile.Path);
        }

        public static async void SaveInfinityOrder(string order) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("infinity", "order", order, iniFile.Path);
        }

        public static async void SaveOneOrder(string order) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("one", "order", order, iniFile.Path);
        }

        public static async Task<StorageFile> GetIniPath() {
            return await GenerateIniFileAsync();
        }

        public static Ini GetIni() {
            string iniFile = GetIniFile();
            Debug.WriteLine("ini: " + FILE_INI);
            Ini ini = new Ini();
            if (iniFile == null) { // 尚未初始化
                return ini;
            }
            StringBuilder sb = new StringBuilder(1024);
            _ = GetPrivateProfileString("timelinewallpaper", "provider", "bing", sb, 1024, iniFile);
            ini.Provider = sb.ToString();
            //_ = GetPrivateProfileString("timelinewallpaper", "push", "", sb, 1024, iniFile);
            //ini.Push = sb.ToString();
            //_ = GetPrivateProfileString("timelinewallpaper", "pushprovider", "bing", sb, 1024, iniFile);
            //ini.PushProvider = sb.ToString();
            _ = GetPrivateProfileString("timelinewallpaper", "desktopprovider", "", sb, 1024, iniFile);
            ini.DesktopProvider = sb.ToString();
            _ = GetPrivateProfileString("timelinewallpaper", "lockprovider", "", sb, 1024, iniFile);
            ini.LockProvider = sb.ToString();
            _ = GetPrivateProfileString("timelinewallpaper", "theme", "", sb, 1024, iniFile);
            ini.Theme = sb.ToString();
            _ = GetPrivateProfileString("bing", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out int desktopPeriod);
            _ = GetPrivateProfileString("bing", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out int lockPeriod);
            _ = GetPrivateProfileString("bing", "lang", "", sb, 1024, iniFile);
            ini.SetIni("bing", new BingIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod,
                Lang = sb.ToString()
            });
            _ = GetPrivateProfileString("nasa", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("nasa", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            _ = GetPrivateProfileString("nasa", "mirror", "", sb, 1024, iniFile);
            ini.SetIni("nasa", new NasaIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod,
                Mirror = sb.ToString()
            });
            _ = GetPrivateProfileString("oneplus", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("oneplus", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            _ = GetPrivateProfileString("oneplus", "order", "date", sb, 1024, iniFile);
            ini.SetIni("oneplus", new OneplusIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod,
                Order = sb.ToString()
            });
            _ = GetPrivateProfileString("timeline", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("timeline", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            TimelineIni timelineIni = new TimelineIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            };
            _ = GetPrivateProfileString("timeline", "order", "date", sb, 1024, iniFile);
            timelineIni.Order = sb.ToString();
            _ = GetPrivateProfileString("timeline", "cate", "", sb, 1024, iniFile);
            timelineIni.Cate = sb.ToString();
            _ = GetPrivateProfileString("timeline", "authorize", "1", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out int authorize);
            timelineIni.Authorize = authorize;
            ini.SetIni("timeline", timelineIni);
            _ = GetPrivateProfileString("ymyouli", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("ymyouli", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            YmyouliIni ymyouliIni = new YmyouliIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            };
            _ = GetPrivateProfileString("ymyouli", "order", "random", sb, 1024, iniFile);
            ymyouliIni.Order = sb.ToString();
            _ = GetPrivateProfileString("ymyouli", "cate", "", sb, 1024, iniFile);
            ymyouliIni.Cate = sb.ToString();
            _ = GetPrivateProfileString("ymyouli", "qc", "1", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out int qc);
            ymyouliIni.Qc = qc;
            ini.SetIni("ymyouli", ymyouliIni);
            _ = GetPrivateProfileString("himawari8", "desktopperiod", "1", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("himawari8", "lockperiod", "2", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            _ = GetPrivateProfileString("himawari8", "offset", "0", sb, 1024, iniFile);
            _ = float.TryParse(sb.ToString(), out float offset);
            ini.SetIni("himawari8", new Himawari8Ini {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod,
                Offset = offset
            });
            _ = GetPrivateProfileString("infinity", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("infinity", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            _ = GetPrivateProfileString("infinity", "order", "", sb, 1024, iniFile);
            ini.SetIni("infinity", new InfinityIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod,
                Order = sb.ToString()
            });
            _ = GetPrivateProfileString("one", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("one", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            _ = GetPrivateProfileString("one", "order", "date", sb, 1024, iniFile);
            ini.SetIni("one", new OneIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod,
                Order = sb.ToString()
            });
            _ = GetPrivateProfileString("3g", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("3g", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            _ = GetPrivateProfileString("3g", "order", "date", sb, 1024, iniFile);
            ini.SetIni("3g", new G3Ini {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod,
                Order = sb.ToString()
            });
            _ = GetPrivateProfileString("bing", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("bing", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            ini.SetIni("bobo", new BoboIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            });
            _ = GetPrivateProfileString("lofter", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("lofter", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            ini.SetIni("lofter", new LofterIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            });
            _ = GetPrivateProfileString("abyss", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("abyss", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            ini.SetIni("abyss", new AbyssIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            });
            _ = GetPrivateProfileString("daihan", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("daihan", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            ini.SetIni("daihan", new DaihanIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            });
            _ = GetPrivateProfileString("bing", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("bing", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            ini.SetIni("dmoe", new DmoeIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            });
            _ = GetPrivateProfileString("toubiec", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("toubiec", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            ini.SetIni("toubiec", new ToubiecIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            });
            _ = GetPrivateProfileString("seovx", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("seovx", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            _ = GetPrivateProfileString("seovx", "cate", "d", sb, 1024, iniFile);
            ini.SetIni("seovx", new SeovxIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod,
                Cate = sb.ToString()
            });
            _ = GetPrivateProfileString("paul", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("paul", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            ini.SetIni("paul", new PaulIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            });
            return ini;
        }

        public static async Task<Ini> GetIniAsync() {
            _ = await GenerateIniFileAsync();
            return GetIni();
        }
    }

    public class DateUtil {
        public static DateTime FromUnixMillis(long unixMillis) {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddMilliseconds(unixMillis);
        }

        public static long ToUnixMillis(DateTime date) {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (long)diff.TotalMilliseconds;
        }

        public static long CurrentTimeMillis() {
            return ToUnixMillis(DateTime.Now);
        }

        public static int GetDaysOfMonth(DateTime date) {
            int month = date.Month;
            if (month == 1 || month == 3 || month == 5 || month == 7
                || month == 8 || month == 10 || month == 12) {
                return 31;
            } else if (month == 2) {
                return (date.Year % 4 == 0 && date.Year % 100 != 0)
                    || date.Year % 400 == 0 ? 29 : 28;
            }
            return 30;
        }

        public static DateTime ParseDate(string text) {
            DateTime date = DateTime.Now;
            if (string.IsNullOrEmpty(text)) {
                return date;
            }
            if (Regex.Match(text, @"\d").Success) {
                if (text.Length == 8) {
                    text = text.Substring(0, 4) + "-" + text.Substring(4, 2) + "-" + text.Substring(6);
                } else if (text.Length == 6) {
                    text = text.Substring(0, 2) + "-" + text.Substring(2, 2) + "-" + text.Substring(4);
                } else if (text.Length == 5) {
                    text = text.Substring(0, 2) + "-" + text.Substring(2, 1) + "-" + text.Substring(3);
                } else if (text.Length == 4) {
                    text = text.Substring(0, 2) + "-" + text.Substring(2);
                } else if (text.Length == 3) {
                    text = text.Substring(0, 1) + "-" + text.Substring(1);
                } else if (text.Length == 2) {
                    if (int.Parse(text) > DateTime.DaysInMonth(date.Year, date.Month)) {
                        text = text.Substring(0, 1) + "-" + text.Substring(1);
                    } else {
                        text = date.Month + "-" + text;
                    }
                } else if (text.Length == 1) {
                    text = date.Month + "-" + text;
                }
            }
            DateTime.TryParse(text, out date);
            return date;
        }
    }

    public class FileUtil {
        public static string MakeValidFileName(string text, string replacement = "_") {
            StringBuilder str = new StringBuilder();
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach (char c in text) {
                if (invalidFileNameChars.Contains(c)) {
                    _ = str.Append(replacement ?? "");
                } else {
                    _ = str.Append(c);
                }
            }

            return str.ToString();
        }

        public static string ConvertFileSize(long size) {
            if (size < 1024) {
                return size + "B";
            }
            if (size < 1024 * 1024) {
                return (size / 1024.0).ToString("0KB");
            }
            if (size < 1024 * 1024 * 1024) {
                return (size / 1024.0 / 1024.0).ToString("0.0MB");
            }
            return (size / 1024.0 / 1024.0 / 1024.0).ToString("0.00GB");
        }
    }

    public class VerUtil {
        public static string GetPkgVer(bool forShort) {
            if (forShort) {
                return string.Format("{0}.{1}",
                    Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor);
            }
            return string.Format("{0}.{1}.{2}.{3}",
                Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor,
                Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
        }

        public static string GetOsVer() {
            ulong version = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            ulong major = (version & 0xFFFF000000000000L) >> 48;
            ulong minor = (version & 0x0000FFFF00000000L) >> 32;
            ulong build = (version & 0x00000000FFFF0000L) >> 16;
            ulong revision = (version & 0x000000000000FFFFL);
            return $"{major}.{minor}.{build}.{revision}";
        }

        public static string GetDevice() {
            var deviceInfo = new EasClientDeviceInformation();
            if (deviceInfo.SystemSku.Length > 0) {
                return deviceInfo.SystemSku;
            }
            return string.Format("{0}_{1}", deviceInfo.SystemManufacturer,
                deviceInfo.SystemProductName);
        }

        public static string GetDeviceId() {
            SystemIdentificationInfo systemId = SystemIdentification.GetSystemIdForPublisher();
            // Make sure this device can generate the IDs
            if (systemId.Source != SystemIdentificationSource.None) {
                // The Id property has a buffer with the unique ID
                DataReader dataReader = DataReader.FromBuffer(systemId.Id);
                return dataReader.ReadGuid().ToString();
            }
            return "";
        }
    }

    public static class ThemeUtil {
        public static ElementTheme ParseTheme(string theme) {
            switch (theme) {
                case "light":
                    return ElementTheme.Light;
                case "dark":
                    return ElementTheme.Dark;
                default:
                    //return ElementTheme.Default; // 该值非系统主题值
                    var uiSettings = new Windows.UI.ViewManagement.UISettings();
                    var color = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
                    if (color == Windows.UI.Color.FromArgb(0xff, 0xff, 0xff, 0xff)) {
                        return ElementTheme.Light;
                    }
                    return ElementTheme.Dark;
            }
        }
    }

    public static class TextUtil {
        public static void Copy(string content) {
            DataPackage pkg = new DataPackage {
                RequestedOperation = DataPackageOperation.Copy
            };
            pkg.SetText(content);
            Clipboard.SetContent(pkg);
        }
    }
}
