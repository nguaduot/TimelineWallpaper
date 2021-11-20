using TimelineWallpaper.Beans;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TimelineWallpaper.Utils;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace TimelineWallpaper.Providers {
    public class MxgProvider : BaseProvider {
        // 随机壁纸 - 木小果API
        // https://api.muxiaoguo.cn/doc/sjbz.html
        private const string URL_API = "https://api.muxiaoguo.cn/api/sjbz?method=pc&type=sina";

        public MxgProvider() {
            Id = "muxiaoguo";
        }

        private Meta ParseBean(MxgApiData bean) {
            Meta meta = new Meta {
                Date = DateTime.Now
            };
            if (bean.ImgUrl != null) {
                Uri uri = new Uri(bean.ImgUrl);
                string[] name = uri.Segments[uri.Segments.Length - 1].Split(".");
                meta.Id = name[0];
                meta.Format = "." + name[1];
                meta.Uhd = Regex.Replace(uri.AbsoluteUri, @"(?<=\.sinaimg\.cn/)[^/]+", "large");
                meta.Thumb = Regex.Replace(uri.AbsoluteUri, @"(?<=\.sinaimg\.cn/)[^/]+", "middle");
            }
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

            string uriApi = string.Format(URL_API, ini.Seovx.Cate);
            Debug.WriteLine("provider url: " + uriApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(URL_API);
                Debug.WriteLine("provider data: " + jsonData);
                MxgApi mxgApi = JsonConvert.DeserializeObject<MxgApi>(jsonData);
                Meta meta = ParseBean(mxgApi.Data);
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

    public class MxgAcgProvider : BaseProvider {
        // 动漫图片 - 木小果API
        // https://api.muxiaoguo.cn/doc/ACG.html
        private const string URL_API = "https://api.muxiaoguo.cn/api/ACG?type=json&size=mw1024";

        public MxgAcgProvider() {
            Id = "muxiaoguo2";
        }

        private Meta ParseBean(MxgApiData bean) {
            Meta meta = new Meta {
                Thumb = bean.ImgUrl,
                Date = DateTime.Now
            };
            if (bean.ImgUrl != null) {
                Uri uri = new Uri(bean.ImgUrl);
                string[] name = uri.Segments[uri.Segments.Length - 1].Split(".");
                meta.Id = name[0];
                meta.Format = "." + name[1];
                meta.Uhd = Regex.Replace(uri.AbsoluteUri, @"(?<=\.sinaimg\.cn/)[^/]+", "large");
            }
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

            string uriApi = string.Format(URL_API, ini.Seovx.Cate);
            Debug.WriteLine("provider url: " + uriApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(URL_API);
                Debug.WriteLine("provider data: " + jsonData);
                MxgApi mxgApi = JsonConvert.DeserializeObject<MxgApi>(jsonData);
                Meta meta = ParseBean(mxgApi.Data);
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

    public class MxgMvProvider : BaseProvider {
        // 美女图片 - 木小果API
        // https://api.muxiaoguo.cn/doc/meinvtu.html
        private const string URL_API = "https://api.muxiaoguo.cn/api/meinvtu?num=30";

        public MxgMvProvider() {
            Id = "muxiaoguo3";
        }

        private Meta ParseBean(MxgApiData bean) {
            Meta meta = new Meta { 
                Uhd = bean.ImgUrl,
                Thumb = bean.ImgUrl,
                Date = DateTime.Now,
                Format = ".jpg"
            };
            if (bean.ImgUrl != null) {
                Uri uri = new Uri(bean.ImgUrl);
                meta.Id = uri.Segments[uri.Segments.Length - 1];
            }
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

            string uriApi = string.Format(URL_API, ini.Seovx.Cate);
            Debug.WriteLine("provider url: " + uriApi);
            try {
                HttpClient client = new HttpClient();
                string jsonData = await client.GetStringAsync(URL_API);
                Debug.WriteLine("provider data: " + jsonData);
                MxgMvApi mxgApi = JsonConvert.DeserializeObject<MxgMvApi>(jsonData);
                foreach (MxgApiData item in mxgApi.Data) {
                    Meta meta = ParseBean(item);
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
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }

            return metas.Count > 0;
        }
    }
}
