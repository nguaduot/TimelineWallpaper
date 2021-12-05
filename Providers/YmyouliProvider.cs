using TimelineWallpaper.Beans;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Linq;
using System.Text.RegularExpressions;

namespace TimelineWallpaper.Providers {
    public class YmyouliProvider : BaseProvider {
        // 下一页数据索引（从0开始）（用于按需加载）
        private int nextPage = 0;

        // 栏目ID表
        private string[] modules = null;

        public static readonly Dictionary<string, string[]> COL_MODULE_DIC = new Dictionary<string, string[]>{
            { "126", new string[]{ "577", "606", "607", "611", "681" } }, // 4K图片 - 游戏动漫人物
            { "127", new string[]{ "677" } }, // 4K图片 - 游戏动漫场景
            { "128", new string[]{ "682" } }, // 4K图片 - 古风人物
            { "134", new string[]{ "678" } }, // 4K图片 - 自然风景
            { "182", new string[]{
                "575", "610", "695", "743", "744", "768",
                "776", "786", "787", "792", "833", "834"
            } }, // 8K图片 - 游戏动漫人物
            { "183", new string[]{ "673", "777" } }, // 8K图片 - 游戏动漫场景
            { "184", new string[]{ "675", "791" } }, // 8K图片 - 自然风景
            { "185", new string[]{ "578", "679", "680", "754" } }, // 8K图片 - 花草植物
            { "186", new string[]{ "753" } }, // 8K图片 - 美女女孩
            { "187", new string[]{ "670", "741", "790" } }, // 8K图片 - 机车
            { "214", new string[]{ "690", "691" } }, // 8K图片 - 科幻
            { "215", new string[]{ "693", "694", "742", "836" } }, // 8K图片 - 意境
            { "224", new string[]{ "746" } }, // 8K图片 - 武器刀剑
            { "225", new string[]{ "748" } }, // 8K图片 - 动物
            { "226", new string[]{ "751" } }, // 8K图片 - 古风人物
            { "227", new string[]{ "756", "773" } }, // 8K图片 - 日暮云天
            { "228", new string[]{ "758" } }, // 8K图片 - 夜空星河
            { "229", new string[]{ "760", "761", "762" } }, // 8K图片 - 战场战争
            { "230", new string[]{ "763" } }, // 8K图片 - 冰雪之境
            { "231", new string[]{ "766" } }, // 8K图片 - 油画
            { "232", new string[]{ "775" } }, // 8K图片 - 国漫壁纸
            { "233", new string[]{ "778" } }, // 8K图片 - 美食蔬果
            { "241", new string[]{ "830" } } // 8K图片 - 樱落
        };

        // 原图下载：http://27146103.s21d-27.faiusrd.com/0/{0}.jpg?f={1}.jpg
        private const string URL_UHD = "https://27146103.s21i.faiusr.com/2/{0}";
        private const string URL_THUMB = "https://27146103.s21i.faiusr.com/2/{0}!1000x1000";

        // 一梦幽黎 4K图片、8K图片
        // https://www.ymyouli.com/
        private const string URL_API = "https://www.ymyouli.com/ajax/ajaxLoadModuleDom_h.jsp";

        public YmyouliProvider() {
            Id = ProviderYmyouli.ID;
        }

        private Meta ParseBean(string id, string name, long time) {
            return new Meta {
                Id = id,
                Uhd = string.Format(URL_UHD, id),
                Thumb = string.Format(URL_THUMB, id),
                Caption = name?.Replace(Regex.Escape("\x00a0"), ""),
                Date = DateUtil.FromUnixMillis(time),
                Format = ".jpg" // TODO
            };
        }

        public override async Task<bool> LoadData(Ini ini) {
            string col = ini.Ymyouli.Col;
            if (col.Length == 0) {
                List<string> cols = Enumerable.ToList(COL_MODULE_DIC.Keys);
                col = cols[new Random().Next(cols.Count)];
            }
            if (modules == null) {
                modules = (string[])COL_MODULE_DIC[col].Clone();
                RandomModules(modules);
            }
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus + 1 < metas.Count || nextPage >= modules.Length) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            string module = modules[nextPage++];
            Dictionary<string, string> formData = new Dictionary<string, string>() {
                { "cmd", "getWafNotCk_getAjaxPageModuleInfo" },
                { "href", string.Format("/col.jsp?id={0}&m{1}pageno=1", col, module) },
                { "_colId", col },
                { "moduleId", module }
            };
            Debug.WriteLine("provider url: " + URL_API);
            try {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("timelinewallpaper", VerUtil.GetPkgVer(true)));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.PostAsync(URL_API, new FormUrlEncodedContent(formData));
                string jsonData = await response.Content.ReadAsStringAsync(); // 首末有多余换行符
                jsonData = Regex.Replace(jsonData, @"^.*?(\{.+\}).*$", "$1", RegexOptions.Singleline); // 提取JSON
                //Debug.WriteLine("provider data: " + jsonData);
                YmyouliApi ymyouliApi = JsonConvert.DeserializeObject<YmyouliApi>(jsonData);
                foreach (YmyouliApiProp5 prop in ymyouliApi.ModuleInfo.Props) {
                    Meta meta = ParseBean(prop.Id, ymyouliApi.ModuleInfo.Name, ymyouliApi.ModuleInfo.CreateTime);
                    if (!meta.IsValid()) {
                        continue;
                    }
                    bool exists = false;
                    foreach (Meta m in metas) {
                        exists |= meta.Id.Equals(m.Id);
                    }
                    if (!exists) {
                        metas.Add(meta);
                    }
                }
                RandomMetas();
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }

        public static void RandomModules(string[] modules) {
            int last = modules.Length - 1;
            for (int i = 0; i < modules.Length; ++i) {
                int ri = new Random().Next(modules.Length - i);
                string temp = modules[last];
                modules[last] = modules[ri];
                modules[ri] = temp;
                last--;
            }
        }
    }
}
