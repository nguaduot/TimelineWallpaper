using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TimelineWallpaper.Beans;
using TimelineWallpaper.Utils;
using Windows.ApplicationModel;
using Windows.Services.Store;
using Windows.System.Profile;
using Windows.System.UserProfile;

namespace TimelineWallpaper.Services {
    public class ApiService {
        public const string URI_STORE = "ms-windows-store://pdp/?productid=9N7VHQ989BB7";

        public static async void Stats(Ini ini, bool status) {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return;
            }
            const string URL_API_STATS = "https://api.nguaduot.cn/appstats";
            StatsApiReq req = new StatsApiReq {
                App = Package.Current.DisplayName, // 不会随语言改变
                Package = Package.Current.Id.FamilyName,
                Version = VerUtil.GetPkgVer(false),
                Api = ini?.ToString(),
                Status = status ? 1 : 0,
                Os = AnalyticsInfo.VersionInfo.DeviceFamily,
                OsVersion = VerUtil.GetOsVer(),
                Device = VerUtil.GetDevice(),
                DeviceId = VerUtil.GetDeviceId(),
                Region = GlobalizationPreferences.HomeGeographicRegion
            };
            try {
                HttpClient client = new HttpClient();
                HttpContent content = new StringContent(JsonConvert.SerializeObject(req),
                    Encoding.UTF8, "application/json");
                //content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync(URL_API_STATS, content);
                _ = response.EnsureSuccessStatusCode();
                string jsonData = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("stats: " + jsonData.Trim());
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
        }

        public static async void Rank(Ini ini, Meta meta, string action) {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return;
            }
            const string URL_API_RANK = "https://api.nguaduot.cn/appstats/rank";
            RankApiReq req = new RankApiReq {
                Provider = ini?.Provider,
                ImgId = meta?.Id,
                ImgUrl = meta?.Uhd,
                Action = action,
                DeviceId = VerUtil.GetDeviceId(),
                Region = GlobalizationPreferences.HomeGeographicRegion
            };
            try {
                HttpClient client = new HttpClient();
                HttpContent content = new StringContent(JsonConvert.SerializeObject(req),
                    Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(URL_API_RANK, content);
                _ = response.EnsureSuccessStatusCode();
                string jsonData = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("rank: " + jsonData.Trim());
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
        }

        public static async Task<bool> Contribute(ContributeApiReq req) {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }
            const string URL_API_CONTRIBUTE = "https://api.nguaduot.cn/timeline/contribute";
            req.AppVer = VerUtil.GetPkgVer(false);
            try {
                HttpClient client = new HttpClient();
                HttpContent content = new StringContent(JsonConvert.SerializeObject(req),
                    Encoding.UTF8, "application/json");
                //content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync(URL_API_CONTRIBUTE, content);
                _ = response.EnsureSuccessStatusCode();
                string jsonData = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("stats: " + jsonData.Trim());
                return jsonData.Contains(@"""status"":1");
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
            return false;
        }

        public static async Task<ReleaseApi> CheckUpdate() {
            if ("fp51msqsmzpvr".Equals(Package.Current.Id.PublisherId)) {
                return await CheckUpdateFromStore();
            }
            return await CheckUpdateFromGithub();
        }

        public static async Task<ReleaseApi> CheckUpdateFromStore() {
            StoreContext context = StoreContext.GetDefault();
            IReadOnlyList<StorePackageUpdate> updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();
            if (updates.Count > 0) {
                return new ReleaseApi {
                    Version = "",
                    Url = URI_STORE
                };
            }
            return null;
        }

        public static async Task<ReleaseApi> CheckUpdateFromGithub() {
            const string URL_RELEASE = "https://api.github.com/repos/nguaduot/TimelineWallpaper/releases/latest";
            try {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("timelinewallpaper", VerUtil.GetPkgVer(true)));
                string jsonData = await client.GetStringAsync(URL_RELEASE);
                Debug.WriteLine("release: " + jsonData.Trim());
                GithubApi api = JsonConvert.DeserializeObject<GithubApi>(jsonData);
                string[] versions = api.TagName.Split(".");
                if (versions.Length < 2) {
                    return null;
                }
                int major = Package.Current.Id.Version.Major;
                int minor = Package.Current.Id.Version.Minor;
                _ = int.TryParse(versions[0], out int majorNew);
                _ = int.TryParse(versions[1], out int minorNew);
                if (majorNew < major || (majorNew == major && minorNew <= minor)) {
                    return null;
                }
                return new ReleaseApi {
                    Version = " v" + majorNew + "." + minorNew,
                    Url = api.Url
                };
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
            return null;
        }
    }
}
