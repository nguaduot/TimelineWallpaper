using System.Collections.Generic;
using TimelineWallpaper.Providers;

namespace TimelineWallpaper.Utils {
    public class Ini {
        public static HashSet<string> PROVIDER = new HashSet<string>() {
            ProviderBing.ID, ProviderNasa.ID, ProviderOnePlus.ID, ProviderTimeline.ID,
            ProviderYmyouli.ID, ProviderInfinity.ID, Provider3G.ID, ProviderPixivel.ID, ProviderLofter.ID,
            ProviderDaihan.ID, ProviderDmoe.ID, ProviderToubiec.ID, ProviderMty.ID, ProviderSeovx.ID,
            ProviderMxg.ID, ProviderPaul.ID
        };
        public static HashSet<string> PUSH = new HashSet<string>() { "", "desktop", "lock" };

        private string provider = ProviderBing.ID;
        public string Provider {
            set => provider = PROVIDER.Contains(value) ? value : ProviderBing.ID;
            get => provider;
        }

        private string push = "";
        public string Push {
            set => push = PUSH.Contains(value) ? value : "";
            get => push;
        }

        private int period = 24;
        public int Period {
            set => period = value <= 0 || value > 24 ? 24 : value;
            get => period;
        }

        public ProviderBing Bing { set; get; } = new ProviderBing();

        public ProviderNasa Nasa { set; get; } = new ProviderNasa();

        public ProviderOnePlus OnePlus { set; get; } = new ProviderOnePlus();

        public ProviderTimeline Timeline { set; get; } = new ProviderTimeline();

        public ProviderYmyouli Ymyouli { set; get; } = new ProviderYmyouli();

        public ProviderInfinity Infinity { set; get; } = new ProviderInfinity();

        public Provider3G G3 { set; get; } = new Provider3G();

        public ProviderPixivel Pixivel { set; get; } = new ProviderPixivel();

        public ProviderLofter Lofter { set; get; } = new ProviderLofter();

        public ProviderDaihan Daihan { set; get; } = new ProviderDaihan();

        public ProviderDmoe Dmoe { set; get; } = new ProviderDmoe();

        public ProviderToubiec Toubiec { set; get; } = new ProviderToubiec();

        public ProviderMty Mty { set; get; } = new ProviderMty();

        public ProviderSeovx Seovx { set; get; } = new ProviderSeovx();

        public ProviderMxg Mxg { set; get; } = new ProviderMxg();

        public ProviderPaul Paul { set; get; } = new ProviderPaul();

        public BaseProvider GenerateProvider() {
            switch (Provider) {
                case ProviderNasa.ID:
                    if ("bjp".Equals(Nasa.Mirror)) {
                        return new NasabjpProvider();
                    }
                    return new NasaProvider();
                case ProviderOnePlus.ID:
                    return new OneplusProvider();
                case ProviderTimeline.ID:
                    return new TimelineProvider();
                case ProviderYmyouli.ID:
                    return new YmyouliProvider();
                case ProviderInfinity.ID:
                    return new InfinityProvider();
                case Provider3G.ID:
                    return new G3Provider();
                case ProviderPixivel.ID:
                    return new PixivelProvider();
                case ProviderLofter.ID:
                    return new LofterProvider();
                case ProviderDaihan.ID:
                    return new DaihanProvider();
                case ProviderDmoe.ID:
                    return new DmoeProvider();
                case ProviderToubiec.ID:
                    return new ToubiecProvider();
                case ProviderMty.ID:
                    return new MtyProvider();
                case ProviderSeovx.ID:
                    return new SeovxProvider();
                case ProviderMxg.ID:
                    if ("acg".Equals(Mxg.Cate)) {
                        return new MxgAcgProvider();
                    } else if ("meinvtu".Equals(Mxg.Cate)) {
                        return new MxgMvProvider();
                    }
                    return new MxgProvider();
                case ProviderPaul.ID:
                    return new PaulProvider();
                case ProviderBing.ID:
                default:
                    return new BingProvider();
            }
        }

        override public string ToString() {
            string paras;
            switch (Provider) {
                case ProviderNasa.ID:
                    paras = Nasa.ToString();
                    break;
                case ProviderOnePlus.ID:
                    paras = OnePlus.ToString();
                    break;
                case ProviderTimeline.ID:
                    paras = Timeline.ToString();
                    break;
                case ProviderYmyouli.ID:
                    paras = Ymyouli.ToString();
                    break;
                case ProviderInfinity.ID:
                    paras = Infinity.ToString();
                    break;
                case Provider3G.ID:
                    paras = G3.ToString();
                    break;
                case ProviderPixivel.ID:
                    paras = Pixivel.ToString();
                    break;
                case ProviderLofter.ID:
                    paras = Lofter.ToString();
                    break;
                case ProviderDaihan.ID:
                    paras = Daihan.ToString();
                    break;
                case ProviderDmoe.ID:
                    paras = Dmoe.ToString();
                    break;
                case ProviderToubiec.ID:
                    paras = Toubiec.ToString();
                    break;
                case ProviderMty.ID:
                    paras = Mty.ToString();
                    break;
                case ProviderSeovx.ID:
                    paras = Seovx.ToString();
                    break;
                case ProviderMxg.ID:
                    paras = Mxg.ToString();
                    break;
                case ProviderPaul.ID:
                    paras = Paul.ToString();
                    break;
                case ProviderBing.ID:
                default:
                    paras = Bing.ToString();
                    break;
            }
            return $"/{provider}?push={push}&period={period}" + (paras.Length > 0 ? "&" : "") + paras;
        }
    }

    public class ProviderBing {
        public const string ID = "bing";
        public static HashSet<string> LANG = new HashSet<string>() { "", "zh-cn", "en-us", "ja-jp", "de-de", "fr-fr" };

        private string lang = "";
        public string Lang {
            set => lang = LANG.Contains(value) ? value : "";
            get => lang;
        }

        override public string ToString() => $"lang={lang}";
    }

    public class ProviderNasa {
        public const string ID = "nasa";
        public static HashSet<string> MIRROR = new HashSet<string>() { "", "bjp" };

        private string mirror = "";
        public string Mirror {
            set => mirror = MIRROR.Contains(value) ? value : "";
            get => mirror;
        }

        override public string ToString() => $"mirror={mirror}";
    }

    public class ProviderOnePlus {
        public const string ID = "oneplus";
        public static HashSet<string> ORDER = new HashSet<string>() { "date", "rate", "view" };

        private string order = "date";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "date";
            get => order;
        }

        override public string ToString() => $"order={order}";
    }

    public class ProviderTimeline {
        public const string ID = "timeline";
        public static HashSet<string> ORDER = new HashSet<string>() { "date", "random" };
        public static HashSet<string> CATE = new HashSet<string>() { "", "landscape", "portrait", "culture" };

        private string order = "date";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "date";
            get => order;
        }

        private string cate = "";
        public string Cate {
            set => cate = CATE.Contains(value) ? value : "";
            get => cate;
        }

        override public string ToString() => $"order={order}&cate={cate}";
    }

    public class ProviderYmyouli {
        public const string ID = "ymyouli";
        public static readonly Dictionary<string, Dictionary<string, string>> COL_MODULE_DIC = new Dictionary<string, Dictionary<string, string>> {
            { "182", new Dictionary<string, string> {
                { "577", "126" },
                { "606", "126" },
                { "607", "126" },
                { "611", "126" },
                { "681", "126" },
                { "575", "182" },
                { "610", "182" },
                { "695", "182" },
                { "743", "182" },
                { "744", "182" },
                { "768", "182" },
                { "776", "182" },
                { "786", "182" },
                { "787", "182" },
                { "792", "182" },
                { "833", "182" },
                { "834", "182" }
            } }, // 游戏动漫人物（4K+8K）
            { "183", new Dictionary<string, string> {
                { "677", "127" },
                { "673", "183" },
                { "777", "183" }
            } }, // 游戏动漫场景（4K+8K）
            { "184", new Dictionary<string, string> {
                { "678", "134" },
                { "675", "184" },
                { "791", "184" }
            } }, // 自然风景（4K+8K）
            { "185", new Dictionary<string, string> {
                { "578", "185" },
                { "679", "185" },
                { "680", "185" },
                { "754", "185" }
            } }, // 花草植物
            { "186", new Dictionary<string, string> {
                { "753", "186" }
            } }, // 美女女孩
            { "187", new Dictionary<string, string> {
                { "670", "187" },
                { "741", "187" },
                { "790", "187" }
            } }, // 机车
            { "214", new Dictionary<string, string> {
                { "690", "214" },
                { "691", "214" }
            } }, // 科幻
            { "215", new Dictionary<string, string> {
                { "693", "215" },
                { "694", "215" },
                { "742", "215" },
                { "836", "215" }
            } }, // 意境
            { "224", new Dictionary<string, string> {
                { "746", "224" }
            } }, // 武器刀剑
            { "225", new Dictionary<string, string> {
                { "748", "225" }
            } }, // 动物
            { "226", new Dictionary<string, string> {
                { "682", "128" },
                { "751", "226" }
            } }, // 古风人物（4K+8K）
            { "227", new Dictionary<string, string> {
                { "756", "227" },
                { "773", "227" }
            } }, // 日暮云天
            { "228", new Dictionary<string, string> {
                { "758", "228" }
            } }, // 夜空星河
            { "229", new Dictionary<string, string> {
                { "760", "229" },
                { "761", "229" },
                { "762", "229" }
            } }, // 战场战争
            { "230", new Dictionary<string, string> {
                { "763", "230" }
            } }, // 冰雪之境
            { "231", new Dictionary<string, string> {
                { "766", "231" }
            } }, // 油画
            { "232", new Dictionary<string, string> {
                { "775", "232" }
            } }, // 国漫壁纸
            { "233", new Dictionary<string, string> {
                { "778", "233" }
            } }, // 美食蔬果
            { "241", new Dictionary<string, string> {
                { "830", "241" }
            } } // 樱落
        }; // { col: { module: col } }

        private string col = "";
        public string Col {
            set => col = COL_MODULE_DIC.ContainsKey(value) ? value : "";
            get => col;
        }

        override public string ToString() => $"col={col}";
    }

    public class ProviderInfinity {
        public const string ID = "infinity";
        public static HashSet<string> ORDER = new HashSet<string>() { "", "rate" };

        private string order = "";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "";
            get => order;
        }

        override public string ToString() => $"order={order}";
    }

    public class Provider3G {
        public const string ID = "3g";
        public static HashSet<string> ORDER = new HashSet<string>() { "date", "view" };

        private string order = "date";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "date";
            get => order;
        }

        override public string ToString() => $"order={order}";
    }

    public class ProviderPixivel {
        public const string ID = "pixivel";

        private int sanity = 5;
        public int Sanity {
            set => sanity = value > 0 ? value : 1;
            get => sanity;
        }

        override public string ToString() => $"sanity={sanity}";
    }

    public class ProviderLofter {
        public const string ID = "lofter";

        override public string ToString() => "";
    }

    public class ProviderDaihan {
        public const string ID = "daihan";

        override public string ToString() => "";
    }

    public class ProviderDmoe {
        public const string ID = "dmoe";

        override public string ToString() => "";
    }

    public class ProviderToubiec {
        public const string ID = "toubiec";

        override public string ToString() => "";
    }

    public class ProviderMty {
        public const string ID = "mty";

        override public string ToString() => "";
    }

    public class ProviderSeovx {
        public const string ID = "seovx";
        public static HashSet<string> CATE = new HashSet<string>() { "", "d", "ha" };

        private string cate = "d";
        public string Cate {
            set => cate = CATE.Contains(value) ? value : "d";
            get => cate;
        }

        override public string ToString() => $"cate={cate}";
    }

    public class ProviderMxg {
        public const string ID = "muxiaoguo";
        public static HashSet<string> CATE = new HashSet<string>() { "sjbz", "acg", "meinvtu" };

        private string cate = "sjbz";
        public string Cate {
            set => cate = CATE.Contains(value) ? value : "dsjbz";
            get => cate;
        }

        override public string ToString() => $"cate={cate}";
    }

    public class ProviderPaul {
        public const string ID = "paul";

        override public string ToString() => "";
    }
}
