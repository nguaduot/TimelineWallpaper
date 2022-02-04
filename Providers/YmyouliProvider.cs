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
using Windows.ApplicationModel.Resources;

namespace TimelineWallpaper.Providers {
    public class YmyouliProvider : BaseProvider {
        // 下一页数据索引（从0开始）（用于按需加载）
        private int nextPage = 0;

        // 栏目ID表
        private List<string> modules = null;

        // 原图下载：http://27146103.s21d-27.faiusrd.com/0/{0}.jpg?f={1}.jpg
        private const string URL_UHD = "https://27146103.s21i.faiusr.com/2/{0}";
        private const string URL_THUMB = "https://27146103.s21i.faiusr.com/2/{0}!1000x1000";

        // 一梦幽黎 4K图片、8K图片
        // https://www.ymyouli.com/
        private const string URL_API = "https://www.ymyouli.com/ajax/ajaxLoadModuleDom_h.jsp";

        private Meta ParseBean(string id, string name, string cate, long time) {
            return new Meta {
                Id = id,
                Uhd = string.Format(URL_UHD, id),
                Thumb = string.Format(URL_THUMB, id),
                Caption = name?.Replace(Regex.Escape("\x00a0"), ""),
                Cate = cate,
                Date = DateUtil.FromUnixMillis(time),
                Format = ".jpg" // TODO
            };
        }

        public override async Task<bool> LoadData(BaseIni ini, DateTime? date = null) {
            string col = ((YmyouliIni)ini).Col;
            if (string.IsNullOrEmpty(col)) {
                List<string> cols = Enumerable.ToList(YmyouliIni.COL_MODULE_DIC.Keys);
                col = cols[new Random().Next(cols.Count)];
                modules = null;
            }
            if (modules == null) {
                modules = Enumerable.ToList(YmyouliIni.COL_MODULE_DIC[col].Keys);
                RandomModules(modules);
            }
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus < metas.Count - 1 || nextPage >= modules.Count) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }
            await base.LoadData(ini, date);

            string module = modules[nextPage++];
            col = YmyouliIni.COL_MODULE_DIC[col][module];
            Dictionary<string, string> formData = new Dictionary<string, string>() {
                { "cmd", "getWafNotCk_getAjaxPageModuleInfo" },
                { "href", string.Format("/col.jsp?id={0}&m{1}pageno=1", col, module) },
                { "_colId", col },
                { "moduleId", module }
            };
            Debug.WriteLine("provider url: " + URL_API + " " + col + " " + module);
            try {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("timelinewallpaper", VerUtil.GetPkgVer(true)));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.PostAsync(URL_API, new FormUrlEncodedContent(formData));
                string jsonData = await response.Content.ReadAsStringAsync(); // 首末有多余换行符
                jsonData = Regex.Replace(jsonData, @"^.*?(\{.+\}).*$", "$1", RegexOptions.Singleline); // 提取JSON
                //Debug.WriteLine("provider data: " + jsonData);
                YmyouliApi ymyouliApi = JsonConvert.DeserializeObject<YmyouliApi>(jsonData);
                List<Meta> metasAdd = new List<Meta>();
                foreach (YmyouliApiProp5 prop in ymyouliApi.ModuleInfo.Props) {
                    metasAdd.Add(ParseBean(prop.Id, ymyouliApi.ModuleInfo.Name,
                        ResourceLoader.GetForCurrentView().GetString("YmyouliCol_" + col),
                        ymyouliApi.ModuleInfo.CreateTime));
                }
                RandomMetas(metasAdd);
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }

        public static void RandomModules(List<string> modules) {
            int last = modules.Count - 1;
            for (int i = 0; i < modules.Count; ++i) {
                int ri = new Random().Next(modules.Count - i);
                string temp = modules[last];
                modules[last] = modules[ri];
                modules[ri] = temp;
                last--;
            }
        }
    }
}
