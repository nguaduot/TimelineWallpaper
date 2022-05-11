using System;
using System.Collections.Generic;
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
        private const string FILE_INI = "timelinewallpaper-4.5.ini";

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defValue,
            StringBuilder returnedString, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string section, string key, string value, string filePath);

        private static async Task<StorageFile> GenerateIniFileAsync() {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile iniFile = await folder.TryGetItemAsync(FILE_INI) as StorageFile;
            if (iniFile == null) { // 生成初始配置文件
                FileInfo[] oldFiles = new DirectoryInfo(folder.Path).GetFiles("*.ini");
                Array.Sort(oldFiles, (a, b) => (b as FileInfo).CreationTime.CompareTo((a as FileInfo).CreationTime));
                StorageFile configFile = await Package.Current.InstalledLocation.GetFileAsync("Assets\\Ini\\config.txt");
                iniFile = await configFile.CopyAsync(folder, FILE_INI, NameCollisionOption.ReplaceExisting);
                Debug.WriteLine("copied ini: " + iniFile.Path);
                if (oldFiles.Length > 0) { // 继承设置
                    Debug.WriteLine("inherit: " + oldFiles[0].Name);
                    StringBuilder sb = new StringBuilder(1024);
                    _ = GetPrivateProfileString("timelinewallpaper", "provider", "bing", sb, 1024, oldFiles[0].FullName);
                    _ = WritePrivateProfileString("timelinewallpaper", "provider", sb.ToString(), iniFile.Path);
                    _ = GetPrivateProfileString("timelinewallpaper", "desktopprovider", "", sb, 1024, oldFiles[0].FullName);
                    _ = WritePrivateProfileString("timelinewallpaper", "desktopprovider", sb.ToString(), iniFile.Path);
                    _ = GetPrivateProfileString("timelinewallpaper", "lockprovider", "", sb, 1024, oldFiles[0].FullName);
                    _ = WritePrivateProfileString("timelinewallpaper", "lockprovider", sb.ToString(), iniFile.Path);
                    _ = GetPrivateProfileString("timelinewallpaper", "theme", "", sb, 1024, oldFiles[0].FullName);
                    _ = WritePrivateProfileString("timelinewallpaper", "theme", sb.ToString(), iniFile.Path);
                }
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

        public static async void SaveQingbzOrder(string order) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("qingbz", "order", order, iniFile.Path);
        }

        public static async void SaveQingbzCate(string cate) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("qingbz", "cate", cate, iniFile.Path);
        }

        public static async void SaveObzhiOrder(string order) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("obzhi", "order", order, iniFile.Path);
        }

        public static async void SaveObzhiCate(string cate) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("obzhi", "cate", cate, iniFile.Path);
        }

        public static async void SaveWallhereOrder(string order) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("wallhere", "order", order, iniFile.Path);
        }

        public static async void SaveWallhereCate(string cate) {
            StorageFile iniFile = await GenerateIniFileAsync();
            _ = WritePrivateProfileString("wallhere", "cate", cate, iniFile.Path);
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
            _ = GetPrivateProfileString("ymyouli", "r18", "0", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out int r18);
            ymyouliIni.R18 = r18;
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
            _ = GetPrivateProfileString("qingbz", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("qingbz", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            QingbzIni qingbzIni = new QingbzIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            };
            _ = GetPrivateProfileString("qingbz", "order", "random", sb, 1024, iniFile);
            qingbzIni.Order = sb.ToString();
            _ = GetPrivateProfileString("qingbz", "cate", "", sb, 1024, iniFile);
            qingbzIni.Cate = sb.ToString();
            _ = GetPrivateProfileString("qingbz", "r18", "0", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out r18);
            qingbzIni.R18 = r18;
            ini.SetIni("qingbz", qingbzIni);
            _ = GetPrivateProfileString("obzhi", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("obzhi", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            ObzhiIni obzhiIni = new ObzhiIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            };
            _ = GetPrivateProfileString("obzhi", "order", "random", sb, 1024, iniFile);
            obzhiIni.Order = sb.ToString();
            _ = GetPrivateProfileString("obzhi", "cate", "", sb, 1024, iniFile);
            obzhiIni.Cate = sb.ToString();
            _ = GetPrivateProfileString("obzhi", "r18", "0", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out r18);
            obzhiIni.R18 = r18;
            ini.SetIni("obzhi", obzhiIni);
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
            _ = GetPrivateProfileString("wallhere", "desktopperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out desktopPeriod);
            _ = GetPrivateProfileString("wallhere", "lockperiod", "24", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out lockPeriod);
            WallhereIni wallhereIni = new WallhereIni {
                DesktopPeriod = desktopPeriod,
                LockPeriod = lockPeriod
            };
            _ = GetPrivateProfileString("wallhere", "order", "random", sb, 1024, iniFile);
            wallhereIni.Order = sb.ToString();
            _ = GetPrivateProfileString("wallhere", "cate", "", sb, 1024, iniFile);
            wallhereIni.Cate = sb.ToString();
            _ = GetPrivateProfileString("wallhere", "r18", "0", sb, 1024, iniFile);
            _ = int.TryParse(sb.ToString(), out r18);
            wallhereIni.R18 = r18;
            ini.SetIni("wallhere", wallhereIni);
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

        public static async Task<IList<string>> GetGlitter() {
            try {
                StorageFile file = await Package.Current.InstalledLocation.GetFileAsync("Assets\\Ini\\glitter.txt");
                if (file != null) {
                    return await FileIO.ReadLinesAsync(file);
                }
            } catch (Exception) {
                Debug.WriteLine("read history error");
            }
            return new List<string>();
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
