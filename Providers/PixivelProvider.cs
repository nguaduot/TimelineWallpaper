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
        private const string URL_HOST_PROXY = "https://i.pixiv.re";

        // Pixivel 首页推荐
        // https://pixivel.moe/
        private const string URL_API = "https://api.pixivel.moe/pixiv?type=illust_recommended";

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
                Uri uriUhd = new Uri(meta.Uhd);
                string[] name = uriUhd.Segments[uriUhd.Segments.Length - 1].Split(".");
                meta.Format = name.Length > 1 ? "." + name[name.Length - 1] : ".jpg";
                // 替换国内代理
                if (meta.Uhd != null) {
                    meta.Uhd = URL_HOST_PROXY + uriUhd.AbsolutePath;
                }
                if (meta.Thumb != null) {
                    meta.Thumb = URL_HOST_PROXY + new Uri(meta.Thumb).AbsolutePath;
                }
            }
            return metas;
        }

        public override async Task<bool> LoadData(BaseIni ini, DateTime? date = null) {
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus < metas.Count - 1) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            int sanityLevel = ((PixivelIni)ini).Sanity;
            Debug.WriteLine("provider url: " + URL_API);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(URL_API);
                Debug.WriteLine("provider data: " + jsonData.Trim());
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
