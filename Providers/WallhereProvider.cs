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
    public class WallhereProvider : BaseProvider {
        // 页数据索引（从1开始）（用于按需加载）
        private int pageIndex = 0;

        private const string URL_API = "https://api.nguaduot.cn/wallhere?client=timelinewallpaper&order={0}&cate={1}&r18={2}&page={3}";
        
        private Meta ParseBean(WallhereApiData bean, string order) {
            Meta meta = new Meta {
                Id = bean.ImgId.ToString(),
                Uhd = bean.ImgUrl,
                Thumb = bean.ThumbUrl,
                Cate = bean.CateAlt,
                Date = DateTime.Now,
                SortFactor = "score".Equals(order) ? bean.Score : bean.ImgId
            };
            meta.Title = string.Format("{0} #{1}", bean.CateAlt, bean.CateAltNo);
            meta.Story = bean.Tag?.Replace(",", " ");
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

            string urlApi = string.Format(URL_API, ((WallhereIni)ini).Order, ((WallhereIni)ini).Cate,
                ((WallhereIni)ini).R18, ++pageIndex);
            Debug.WriteLine("provider url: " + urlApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(urlApi);
                Debug.WriteLine("provider data: " + jsonData.Trim());
                WallhereApi api = JsonConvert.DeserializeObject<WallhereApi>(jsonData);
                List<Meta> metasAdd = new List<Meta>();
                foreach (WallhereApiData item in api.Data) {
                    metasAdd.Add(ParseBean(item, ((WallhereIni)ini).Order));
                }
                if ("date".Equals(((WallhereIni)ini).Order) || "score".Equals(((WallhereIni)ini).Order)) { // 有序排列
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
