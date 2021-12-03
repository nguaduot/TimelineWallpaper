using TimelineWallpaper.Beans;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Data.Html;
using System.Collections.Generic;

namespace TimelineWallpaper.Providers {
    public class PixivelProvider : BaseProvider {
        // 下一页数据索引（从1开始）（用于按需加载）
        private int nextPage = 1;

        private const string URL_HOST_PROXY = "https://proxy.pixivel.moe";

        // Pixivel 首页推荐
        // https://pixivel.moe/
        private const string URL_API = "https://api-jp1.pixivel.moe/pixiv?type=illust_recommended";

        public PixivelProvider() {
            Id = ProviderPixivel.ID;
        }

        private List<Meta> ParseBeans(PixivelApiIllust bean) {
            List<Meta> metas = new List<Meta>();
            if (bean.MetaPages != null && bean.MetaPages.Count > 0) {
                for (int i = 0; i < bean.MetaPages.Count; ++i) {
                    PixivelApiPage2 page = bean.MetaPages[i];
                    metas.Add(new Meta {
                        Id = bean.Id + "_" + i,
                        Uhd = page.ImageUrls.Original,
                        Thumb = page.ImageUrls.Medium,
                        Title = bean.Title,
                        Story = HtmlUtilities.ConvertToText(bean.Caption).Replace("\n\n", "\n"),
                        Copyright = "@" + bean.User.Name,
                        Date = DateTime.Parse(bean.CreateDate)
                    });
                }
            } else {
                metas.Add(new Meta {
                    Id = bean.Id,
                    Uhd = bean.MetaSinglePage?.OriginalImageUrl,
                    Thumb = bean.ImageUrls?.Medium,
                    Title = bean.Title,
                    Story = HtmlUtilities.ConvertToText(bean.Caption).Replace("\n\n", "\n"),
                    Copyright = "@" + bean.User.Name,
                    Date = DateTime.Parse(bean.CreateDate)
                });
            }
            foreach (Meta meta in metas) {
                meta.Format = "." + meta.Uhd.Split(".")[1];
                // 替换国内代理
                if (meta.Uhd != null) {
                    meta.Uhd = URL_HOST_PROXY + new Uri(meta.Uhd).AbsolutePath;
                }
                if (meta.Thumb != null) {
                    meta.Thumb = URL_HOST_PROXY + new Uri(meta.Thumb).AbsolutePath;
                }
            }
            return metas;
        }

        public override async Task<bool> LoadData(Ini ini) {
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus + 1 < metas.Count || nextPage++ > 1) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            int sanityLevel = ini.Pixivel.Sanity;
            Debug.WriteLine("provider url: " + URL_API);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(URL_API);
                Debug.WriteLine("provider data: " + jsonData);
                PixivelApi pixivelApi = JsonConvert.DeserializeObject<PixivelApi>(jsonData);
                foreach (PixivelApiIllust illust in pixivelApi.Illusts) {
                    if (illust.SanityLevel > sanityLevel) {
                        continue;
                    }
                    foreach (Meta meta in ParseBeans(illust)) {
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
                }
                RandomMetas();
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
