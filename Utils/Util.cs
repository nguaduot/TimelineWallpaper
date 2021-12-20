using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace TimelineWallpaper.Utils {
    public class IniUtil {
        // TODO: 参数有变动时需调整配置名
        private const string FILE_INI = "timelinewallpaper-2.6.ini";

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
                    "; provider=ymyouli    图源：一梦幽黎 - 本站资源准备历时数年 https://www.ymyouli.com",
                    "; provider=infinity   图源：Infinity - 365天精选壁纸 http://cn.infinitynewtab.com",
                    "; provider=himawari8  图源：向日葵8号 - 实时地球 https://himawari8.nict.go.jp",
                    "; provider=3g         图源：3G壁纸 - 电脑壁纸专家 https://desk.3gbizhi.com",
                    "; provider=pixivel    图源：Pixivel - Pixel 图片缓存/代理 https://pixivel.moe",
                    "; provider=lofter     图源：Lofter - 看见每一种兴趣 https://www.lofter.com",
                    "; provider=daihan     图源：呆憨API - 随机二次元ACG图片 https://api.daihan.top/html/acg.html",
                    "; provider=dmoe       图源：樱花API - 随机二次元图片 https://www.dmoe.cc",
                    "; provider=toubiec    图源：晓晴API - 随机二次元图片 https://acg.toubiec.cn",
                    "; provider=mty        图源：墨天逸API - 随机图片 https://api.mtyqx.cn",
                    "; provider=seovx      图源：夏沫博客API - 在线古风美图二次元 https://cdn.seovx.com",
                    //"; provider=muxiaoguo  图源：木小果API - 随机壁纸 https://api.muxiaoguo.cn",
                    "; provider=paul       图源：保罗API - 随机动漫壁纸 https://api.paugram.com/help/wallpaper",
                    "",
                    "push=",
                    "; push=         推送：关闭推送（默认）",
                    "; push=desktop  推送：推送桌面背景",
                    "; push=lock     推送：推送锁屏背景",
                    "",
                    "pushprovider=bing",
                    "; pushprovider={provider}  推送图源：参数参考 provider",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "theme=",
                    "; theme=       主题：跟随系统（默认）",
                    "; theme=light  主题：亮色",
                    "; theme=dark   主题：暗色",
                    "",
                    "[bing]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
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
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "mirror=bjp",
                    "; mirror=     镜像：无",
                    "; mirror=bjp  镜像：北京天文馆（默认） http://www.bjp.org.cn/mryt",
                    "",
                    "[oneplus]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "order=date",
                    "; order=date  排序：最新添加（默认）",
                    "; order=rate  排序：点赞最多",
                    "; order=view  排序：浏览最多",
                    "",
                    "[timeline]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
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
                    "",
                    "[ymyouli]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "col=",
                    "; col=     类别：随机（默认）",
                    "; col=182  类别：游戏动漫人物",
                    "; col=183  类别：游戏动漫场景",
                    "; col=184  类别：自然风景",
                    "; col=185  类别：花草植物",
                    "; col=186  类别：美女女孩",
                    "; col=187  类别：机车",
                    "; col=214  类别：科幻",
                    "; col=215  类别：意境",
                    "; col=224  类别：武器刀剑",
                    "; col=225  类别：动物",
                    "; col=226  类别：古风人物",
                    "; col=227  类别：日暮云天",
                    "; col=228  类别：夜空星河",
                    "; col=229  类别：战场战争",
                    "; col=230  类别：冰雪之境",
                    "; col=231  类别：油画",
                    "; col=232  类别：国漫壁纸",
                    "; col=233  类别：美食蔬果",
                    "; col=241  类别：樱落",
                    "",
                    "[infinity]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "order=",
                    "; order=      排序：随机（默认）",
                    "; order=rate  排序：热度",
                    "",
                    "[himawari8]",
                    "",
                    "pushperiod=1",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "[3g]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "order=date",
                    "; order=date  排序：最新壁纸（默认）",
                    "; order=view  排序：热门壁纸",
                    "",
                    "[pixivel]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "sanity=5",
                    "; sanity={n}   敏感度：1-10（默认为5）",
                    "",
                    "[lofter]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "[daihan]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "[dmoe]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "[toubiec]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "[seovx]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "cate=d",
                    "; cate=    类别：美图",
                    "; cate=d   类别：二次元（默认）",
                    "; cate=ha  类别：古风",
                    "",
                    //"[muxiaoguo]",
                    //"",
                    //"pushperiod=24",
                    //"; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
                    //"",
                    //"cate=sjbz",
                    //"; cate=sjbz     类别：随机壁纸（默认）",
                    //"; cate=acg      类别：动漫图片",
                    //"; cate=meinvtu  类别：美女图片",
                    //"",
                    "[paul]",
                    "",
                    "pushperiod=24",
                    "; pushperiod={n}  推送周期：1-24（默认为24h/次，开启推送后生效）",
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

        public static async void SavePush(string push) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("timelinewallpaper", "push", push, iniFile.Path);
        }

        public static async void SavePushProvider(string provider) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("timelinewallpaper", "pushprovider", provider, iniFile.Path);
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

        public static async void SaveYmyouliCol(string col) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("ymyouli", "col", col, iniFile.Path);
        }

        public static async void SaveInfinityOrder(string order) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("infinity", "order", order, iniFile.Path);
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
            _ = GetPrivateProfileString("timelinewallpaper", "push", "", sb, 1024, iniFile);
            ini.Push = sb.ToString();
            _ = GetPrivateProfileString("timelinewallpaper", "pushprovider", "bing", sb, 1024, iniFile);
            ini.PushProvider = sb.ToString();
            _ = GetPrivateProfileString("timelinewallpaper", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out int period);
            ini.PushPeriod = period;
            _ = GetPrivateProfileString("timelinewallpaper", "theme", "", sb, 1024, iniFile);
            ini.Theme = sb.ToString();
            _ = GetPrivateProfileString("bing", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            _ = GetPrivateProfileString("bing", "lang", "", sb, 1024, iniFile);
            ini.Inis["bing"] = new BingIni {
                PushPeriod = period,
                Lang = sb.ToString()
            };
            _ = GetPrivateProfileString("nasa", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            _ = GetPrivateProfileString("nasa", "mirror", "", sb, 1024, iniFile);
            ini.Inis["nasa"] = new NasaIni {
                PushPeriod = period,
                Mirror = sb.ToString()
            };
            _ = GetPrivateProfileString("oneplus", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            _ = GetPrivateProfileString("oneplus", "order", "date", sb, 1024, iniFile);
            ini.Inis["oneplus"] = new OneplusIni {
                PushPeriod = period,
                Order = sb.ToString()
            };
            _ = GetPrivateProfileString("timeline", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            _ = GetPrivateProfileString("timeline", "order", "date", sb, 1024, iniFile);
            TimelineIni timelineIni = new TimelineIni {
                PushPeriod = period,
                Order = sb.ToString()
            };
            _ = GetPrivateProfileString("timeline", "cate", "", sb, 1024, iniFile);
            timelineIni.Cate = sb.ToString();
            ini.Inis["timeline"] = timelineIni;
            _ = GetPrivateProfileString("ymyouli", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            _ = GetPrivateProfileString("ymyouli", "col", "", sb, 1024, iniFile);
            ini.Inis["ymyouli"] = new YmyouliIni {
                PushPeriod = period,
                Col = sb.ToString()
            };
            _ = GetPrivateProfileString("infinity", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            _ = GetPrivateProfileString("infinity", "order", "", sb, 1024, iniFile);
            ini.Inis["infinity"] = new InfinityIni {
                PushPeriod = period,
                Order = sb.ToString()
            };
            _ = GetPrivateProfileString("himawari8", "pushperiod", "1", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            ini.Inis["himawari8"] = new Himawari8Ini {
                PushPeriod = period
            };
            _ = GetPrivateProfileString("3g", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            _ = GetPrivateProfileString("3g", "order", "date", sb, 1024, iniFile);
            ini.Inis["3g"] = new G3Ini {
                PushPeriod = period,
                Order = sb.ToString()
            };
            _ = GetPrivateProfileString("pixivel", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            _ = GetPrivateProfileString("pixivel", "sanity", "5", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out int sanity);
            ini.Inis["pixivel"] = new PixivelIni {
                PushPeriod = period,
                Sanity = sanity
            };
            _ = GetPrivateProfileString("lofter", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            ini.Inis["lofter"] = new LofterIni {
                PushPeriod = period
            };
            _ = GetPrivateProfileString("daihan", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            ini.Inis["daihan"] = new DaihanIni {
                PushPeriod = period
            };
            _ = GetPrivateProfileString("dmoe", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            ini.Inis["dmoe"] = new DmoeIni {
                PushPeriod = period
            };
            _ = GetPrivateProfileString("toubiec", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            ini.Inis["toubiec"] = new ToubiecIni {
                PushPeriod = period
            };
            _ = GetPrivateProfileString("seovx", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            _ = GetPrivateProfileString("seovx", "cate", "d", sb, 1024, iniFile);
            ini.Inis["seovx"] = new SeovxIni {
                PushPeriod = period,
                Cate = sb.ToString()
            };
            //_ = GetPrivateProfileString("muxiaoguo", "pushperiod", "24", sb, 1024, iniFile);
            //_ = int.TryParse(sb.ToString(), out period);
            //_ = GetPrivateProfileString("muxiaoguo", "cate", "sjbz", sb, 1024, iniFile);
            //ini.Inis["muxiaoguo"] = new MxgIni {
            //    PushPeriod = period,
            //    Cate = sb.ToString()
            //};
            _ = GetPrivateProfileString("paul", "pushperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out period);
            ini.Inis["paul"] = new PaulIni {
                PushPeriod = period
            };
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
}
