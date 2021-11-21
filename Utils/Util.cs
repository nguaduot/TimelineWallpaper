using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TimelineWallpaper.Utils {
    public class IniUtil {
        private const string FILE_INI = "timelinewallpaper-2.1.ini";

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defValue,
            StringBuilder returnedString, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string section, string key, string value, string filePath);

        private static async Task<StorageFile> GenerateIni() {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile iniFile = await folder.TryGetItemAsync(FILE_INI) as StorageFile;
            if (iniFile == null) {
                iniFile = await folder.CreateFileAsync(FILE_INI, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteLinesAsync(iniFile, new string[] {
                    "; 拾光 v2.1.211121",
                    "; nguaduot@163.com",
                    "",
                    "[timelinewallpaper]",
                    "",
                    "provider=bing",
                    "; provider=bing       图源：Microsoft Bing - 每天发现一个新地方 https://cn.bing.com",
                    "; provider=nasa       图源：NASA - 每日天文一图 https://apod.nasa.gov/apod",
                    "; provider=oneplus    图源：OnePlus - Shot on OnePlus https://photos.oneplus.com",
                    "; provider=timeline   图源：拾光 - 时光如歌，岁月如诗 https://github.com/nguaduot/TimelineApi",
                    "; provider=ymyouli    图源：一梦幽黎 - 本站资源准备历时数年 https://www.ymyouli.com",
                    "; provider=infinity   图源：Infinity - 365天精选壁纸 http://cn.infinitynewtab.com",
                    "; provider=3g         图源：3G壁纸 - 电脑壁纸专家 https://desk.3gbizhi.com",
                    "; provider=pixivel    图源：Pixivel - Pixel 图片缓存/代理 https://pixivel.moe",
                    "; provider=lofter     图源：Lofter - 看见每一种兴趣 https://www.lofter.com",
                    "; provider=daihan     图源：呆憨API - 随机二次元ACG图片 https://api.daihan.top/html/acg.html",
                    "; provider=dmoe       图源：樱花API - 随机二次元图片 https://www.dmoe.cc",
                    "; provider=toubiec    图源：晓晴API - 随机二次元图片 https://acg.toubiec.cn",
                    "; provider=mty        图源：墨天逸API - 随机图片 https://api.mtyqx.cn",
                    "; provider=seovx      图源：夏沫博客API - 在线古风美图二次元 https://cdn.seovx.com",
                    "; provider=muxiaoguo  图源：木小果API - 随机壁纸 https://api.muxiaoguo.cn",
                    "; provider=paul       图源：保罗API - 随机动漫壁纸 https://api.paugram.com/help/wallpaper",
                    "",
                    "push=",
                    "; push=         推送：关闭推送（默认）",
                    "; push=desktop  推送：推送桌面背景",
                    "; push=lock     推送：推送锁屏背景",
                    "",
                    "period=24",
                    "; period={n}  # 推送周期：1-24（默认为24h/次，开启推送后生效）",
                    "",
                    "[bing]",
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
                    "mirror=",
                    "; mirror=     镜像：无（默认）",
                    "; mirror=bjp  镜像：北京天文馆 http://www.bjp.org.cn/mryt",
                    "",
                    "[oneplus]",
                    "",
                    "order=date",
                    "; order=date  排序：最新添加（默认）",
                    "; order=rate  排序：点赞最多",
                    "; order=view  排序：浏览最多",
                    "",
                    "[timeline]",
                    "",
                    "order=date",
                    "; order=date    排序：日期（默认）",
                    "; order=random  排序：随机",
                    "",
                    "cate=",
                    "; cate=           类别：随机（默认）",
                    "; cate=landscape  类别：风光摄影",
                    "; cate=portrait   类别：人像摄影",
                    "; cate=culture    类别：人文摄影",
                    "",
                    "[ymyouli]",
                    "",
                    "col=",
                    "; col=     类别：随机（默认）",
                    "; col=126  类别：4K图片 - 游戏动漫人物",
                    "; col=127  类别：4K图片 - 游戏动漫场景",
                    "; col=128  类别：4K图片 - 古风人物",
                    "; col=134  类别：4K图片 - 自然风景",
                    "; col=182  类别：8K图片 - 游戏动漫人物",
                    "; col=183  类别：8K图片 - 游戏动漫场景",
                    "; col=184  类别：8K图片 - 自然风景",
                    "; col=185  类别：8K图片 - 花草植物",
                    "; col=186  类别：8K图片 - 美女女孩",
                    "; col=187  类别：8K图片 - 机车",
                    "; col=214  类别：8K图片 - 科幻",
                    "; col=215  类别：8K图片 - 意境",
                    "; col=224  类别：8K图片 - 武器刀剑",
                    "; col=225  类别：8K图片 - 动物",
                    "; col=226  类别：8K图片 - 古风人物",
                    "; col=227  类别：8K图片 - 日暮云天",
                    "; col=228  类别：8K图片 - 夜空星河",
                    "; col=229  类别：8K图片 - 战场战争",
                    "; col=230  类别：8K图片 - 冰雪之境",
                    "; col=231  类别：8K图片 - 油画",
                    "; col=232  类别：8K图片 - 国漫壁纸",
                    "; col=233  类别：8K图片 - 美食蔬果",
                    "; col=241  类别：8K图片 - 樱落",
                    "",
                    "[infinity]",
                    "",
                    "order=",
                    "; order=      排序：随机（默认）",
                    "; order=rate  排序：热度",
                    "",
                    "[3g]",
                    "",
                    "order=date",
                    "; order=date  排序：最新壁纸（默认）",
                    "; order=view  排序：热门壁纸",
                    "",
                    "[pixivel]",
                    "",
                    "sanity=5",
                    "; sanity={n}   敏感度：1-10（默认为5）",
                    "",
                    "[lofter]",
                    "",
                    "[daihan]",
                    "",
                    "[dmoe]",
                    "",
                    "[toubiec]",
                    "",
                    "[seovx]",
                    "",
                    "cate=d",
                    "; cate=    类别：美图",
                    "; cate=d   类别：二次元（默认）",
                    "; cate=ha  类别：古风",
                    "",
                    "[muxiaoguo]",
                    "",
                    "cate=sjbz",
                    "; cate=sjbz     类别：随机壁纸（默认）",
                    "; cate=acg      类别：动漫图片",
                    "; cate=meinvtu  类别：美女图片",
                    "",
                    "[paul]",
                    ""
                });
                //StorageFile defFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/timelinewallpaper.ini"));
                //iniFile = await defFile.CopyAsync(ApplicationData.Current.LocalFolder,
                //    iniFileName, NameCollisionOption.ReplaceExisting);
            }
            Debug.WriteLine("ini: " + iniFile.Path);
            return iniFile;
        }

        public static async void SavePush(string push) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("timelinewallpaper", "push", push, iniFile.Path);
        }

        public static async void SaveProvider(string provider) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("timelinewallpaper", "provider", provider, iniFile.Path);
        }

        public static async void SaveMenu(string menu) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("timelinewallpaper", "menu", menu, iniFile.Path);
        }

        public static async void SaveBingLang(string langCode) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("bing", "lang", langCode, iniFile.Path);
        }

        public static async void SaveNasaMirror(string mirror) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("nasa", "mirror", mirror, iniFile.Path);
        }

        public static async void SaveOneplusOrder(string order) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("oneplus", "order", order, iniFile.Path);
        }

        public static async void SaveTimelineOrder(string order) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("timeline", "order", order, iniFile.Path);
        }

        public static async void SaveTimelineCate(string order) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("timeline", "cate", order, iniFile.Path);
        }

        public static async void SaveYmyouliCol(string col) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("ymyouli", "col", col, iniFile.Path);
        }

        public static async void SaveInfinityOrder(string order) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("infinity", "order", order, iniFile.Path);
        }

        public static async void Save3GOrder(string order) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("3g", "order", order, iniFile.Path);
        }

        public static async void SavePixivelSanity(string sanity) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("pixivel", "sanity", sanity, iniFile.Path);
        }

        public static async void SaveSeovxCate(string cate) {
            StorageFile iniFile = await GenerateIni();
            _ = WritePrivateProfileString("seovx", "cate", cate, iniFile.Path);
        }

        public static async Task<StorageFile> GetIniPath() {
            return await GenerateIni();
        }

        public static async Task<Ini> GetIni() {
            StorageFile iniFile = await GenerateIni();
            Ini ini = new Ini();
            StringBuilder sb = new StringBuilder(1024);
            _ = GetPrivateProfileString("timelinewallpaper", "provider", "bing", sb, 1024, iniFile.Path);
            ini.Provider = sb.ToString();
            _ = GetPrivateProfileString("timelinewallpaper", "push", "", sb, 1024, iniFile.Path);
            ini.Push = sb.ToString();
            _ = GetPrivateProfileString("timelinewallpaper", "period", "24", sb, 1024, iniFile.Path);
            _ = int.TryParse(sb.ToString(), out int period);
            ini.Period = period;
            _ = GetPrivateProfileString("bing", "lang", "", sb, 1024, iniFile.Path);
            ini.Bing.Lang = sb.ToString();
            _ = GetPrivateProfileString("nasa", "mirror", "", sb, 1024, iniFile.Path);
            ini.Nasa.Mirror = sb.ToString();
            _ = GetPrivateProfileString("oneplus", "order", "date", sb, 1024, iniFile.Path);
            ini.OnePlus.Order = sb.ToString();
            _ = GetPrivateProfileString("timeline", "order", "date", sb, 1024, iniFile.Path);
            ini.Timeline.Order = sb.ToString();
            _ = GetPrivateProfileString("timeline", "cate", "", sb, 1024, iniFile.Path);
            ini.Timeline.Cate = sb.ToString();
            _ = GetPrivateProfileString("ymyouli", "col", "", sb, 1024, iniFile.Path);
            ini.Ymyouli.Col = sb.ToString();
            _ = GetPrivateProfileString("infinity", "order", "", sb, 1024, iniFile.Path);
            ini.Infinity.Order = sb.ToString();
            _ = GetPrivateProfileString("3g", "order", "date", sb, 1024, iniFile.Path);
            ini.G3.Order = sb.ToString();
            _ = GetPrivateProfileString("pixivel", "sanity", "5", sb, 1024, iniFile.Path);
            _ = int.TryParse(sb.ToString(), out int sanity);
            ini.Pixivel.Sanity = sanity;
            _ = GetPrivateProfileString("seovx", "cate", "d", sb, 1024, iniFile.Path);
            ini.Seovx.Cate = sb.ToString();
            _ = GetPrivateProfileString("muxiaoguo", "cate", "sjbz", sb, 1024, iniFile.Path);
            ini.Mxg.Cate = sb.ToString();
            return ini;
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
}
