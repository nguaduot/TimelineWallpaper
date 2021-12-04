using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TimelineWallpaper.Beans;
using TimelineWallpaper.Utils;
using Windows.ApplicationModel;
using Windows.System.Profile;

namespace TimelineWallpaper.Services {
    public class ApiService {
        public static async void Stats(Ini ini, bool status) {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return;
            }
            //const string URL_API_STATS = "https://api.nguaduot.cn/appstats";
            const string URL_API_STATS = "http://150.158.49.144/appstats";
            StatsApiReq req = new StatsApiReq {
                App = Package.Current.DisplayName,
                Package = Package.Current.Id.FamilyName,
                Version = VerUtil.GetPkgVer(false),
                Api = ini?.ToString(),
                Status = status ? 1 : 0,
                Os = AnalyticsInfo.VersionInfo.DeviceFamily,
                OsVersion = VerUtil.GetOsVer(),
                Device = VerUtil.GetDevice(),
                DeviceId = VerUtil.GetDeviceId()
            };
            try {
                HttpClient client = new HttpClient();
                HttpContent content = new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync(URL_API_STATS, content);
                _ = response.EnsureSuccessStatusCode();
                string jsonData = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("stats: " + jsonData);
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
        }

        public static async Task<GithubApi> CheckUpdate() {
            const string URL_RELEASE = "https://api.github.com/repos/nguaduot/TimelineWallpaper/releases/latest";
            try {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("timelinewallpaper", VerUtil.GetPkgVer(true)));
                string jsonData = await client.GetStringAsync(URL_RELEASE);
                Debug.WriteLine("release: " + jsonData);
                return JsonConvert.DeserializeObject<GithubApi>(jsonData);
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
            return null;
        }
    }
}
