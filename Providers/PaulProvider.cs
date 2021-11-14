using TimelineWallpaper.Beans;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;

namespace TimelineWallpaper.Providers {
    public class PaulProvider : BaseProvider {
        // 随机动漫壁纸 - 保罗 | API
        // 生成适合 Single 主题的白底动漫壁纸
        // https://api.paugram.com/help/wallpaper
        // GET参数：
        // sm: （可用）sm.ms 图床，国外速度较快，部分浏览器会阻止访问
        // cp: （不可用）Coding Pages 服务托管，服务器位于香港，部分移动网络无法访问
        // gt: （可用）Gitee Pages 服务托管，服务器位于国内，速度稳定
        // sina: （可用）新浪微博相册，国内速度较快，已更换新地址
        // gh: （可用）存放在 GitHub 上的图片资源，基于 JSDelivr 托管，速度非常理想
        private const string URL_API = "https://api.paugram.com/wallpaper/?source=gh";

        public PaulProvider() {
            Id = "paul";
        }

        private Meta ParseBean(Uri uriImg) {
            Meta meta = new Meta();
            if (uriImg == null) {
                return meta;
            }
            string[] name = uriImg.Segments[uriImg.Segments.Length - 1].Split(".");
            meta.Id = name[0];
            meta.Format = "." + name[1];
            meta.Uhd = uriImg.AbsoluteUri;
            meta.Thumb = uriImg.AbsoluteUri;
            meta.Date = DateTime.Now;
            return meta;
        }

        public override async Task<bool> LoadData(Ini ini) {
            // 现有数据未浏览完，无需加载更多，或已无更多数据
            if (indexFocus + 1 < metas.Count) {
                return true;
            }
            // 无网络连接
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            Debug.WriteLine("provider url: " + URL_API);
            try {
                HttpClient client = new HttpClient(new HttpClientHandler {
                    AllowAutoRedirect = false
                });
                HttpResponseMessage msg = await client.GetAsync(URL_API);
                Meta meta = ParseBean(msg.Headers.Location);
                if (!meta.IsValid()) {
                    return metas.Count > 0;
                }
                bool exists = false;
                foreach (Meta m in metas) {
                    exists |= meta.Id.Equals(m.Id);
                }
                if (!exists) {
                    metas.Add(meta);
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
