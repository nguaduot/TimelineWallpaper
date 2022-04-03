using TimelineWallpaper.Beans;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TimelineWallpaper.Providers {
    public class YmyouliProvider : BaseProvider {
        // 页数据索引（从1开始）（用于按需加载）
        private int pageIndex = 0;

        private const string URL_API = "https://api.nguaduot.cn/ymyouli?client=timelinewallpaper&cate={0}&order={1}&qc={2}&page={3}";

        private Meta ParseBean(YmyouliApiData bean) {
            Meta meta = new Meta {
                Id = bean.ImgId,
                Uhd = bean.ImgUrl,
                Thumb = bean.ThumbUrl,
                Cate = bean.CateAlt,
                Date = DateTime.Now,
                Format = ".jpg", // TODO
                SortFactor = bean.No
            };
            //meta.Caption = String.Format("{0} · {1}",
            //    ResourceLoader.GetForCurrentView().GetString("Provider_" + this.Id), bean.Cate);
            meta.Title = string.Format("{0} #{1}", bean.CateAlt, bean.CateAltNo);
            meta.Caption = string.Format("{0} · {1}", bean.Cate, bean.Group);
            if (bean.Deprecated != 0) {
                meta.Title = "🚫 " + meta.Title;
            }
            return meta;
        }

        public override async Task<bool> LoadData(BaseIni ini, DateTime? date = null) {
            // 现有数据未浏览完，无需加载更多
            if (indexFocus < metas.Count - 1) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }
            await base.LoadData(ini, date);

            string urlApi = string.Format(URL_API, ((YmyouliIni)ini).Cate, ((YmyouliIni)ini).Order,
                ((YmyouliIni)ini).Qc, ++pageIndex);
            Debug.WriteLine("provider url: " + urlApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(urlApi);
                Debug.WriteLine("provider data: " + jsonData.Trim());
                YmyouliApi ymyouliApi = JsonConvert.DeserializeObject<YmyouliApi>(jsonData);
                List<Meta> metasAdd = new List<Meta>();
                foreach (YmyouliApiData item in ymyouliApi.Data) {
                    metasAdd.Add(ParseBean(item));
                }
                if ("date".Equals(((YmyouliIni)ini).Order)) { // 按时序倒序排列
                    SortMetas(metasAdd);
                } else {
                    AppendMetas(metasAdd);
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
