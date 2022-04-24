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
    public class ObzhiProvider : BaseProvider {
        // 页数据索引（从1开始）（用于按需加载）
        private int pageIndex = 0;

        private const string URL_API = "https://api.nguaduot.cn/obzhi?client=timelinewallpaper&cate={0}&order={1}&r18={2}&page={3}";
        
        private Meta ParseBean(ObzhiApiData bean, string order) {
            Meta meta = new Meta {
                Id = bean.ImgId.ToString(),
                Uhd = bean.ImgUrl,
                Thumb = bean.ThumbUrl,
                Title = bean.Title,
                Story = bean.Story,
                Copyright = "@" + bean.Author,
                Cate = bean.CateAlt,
                Date = DateTime.ParseExact(bean.RelDate, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US")),
                SortFactor = "score".Equals(order) ? bean.Score : bean.ImgId
            };
            if (bean.R18 == 1) {
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

            string urlApi = string.Format(URL_API, ((ObzhiIni)ini).Cate, ((ObzhiIni)ini).Order,
                ((ObzhiIni)ini).R18, ++pageIndex);
            Debug.WriteLine("provider url: " + urlApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(urlApi);
                Debug.WriteLine("provider data: " + jsonData.Trim());
                ObzhiApi obzhiApi = JsonConvert.DeserializeObject<ObzhiApi>(jsonData);
                List<Meta> metasAdd = new List<Meta>();
                foreach (ObzhiApiData item in obzhiApi.Data) {
                    metasAdd.Add(ParseBean(item, ((ObzhiIni)ini).Order));
                }
                if ("date".Equals(((ObzhiIni)ini).Order) || "score".Equals(((ObzhiIni)ini).Order)) { // 有序排列
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
