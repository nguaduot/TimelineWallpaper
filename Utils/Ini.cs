using System.Collections.Generic;
using TimelineWallpaper.Providers;

namespace TimelineWallpaper.Utils {
    public class Ini {
        public readonly Dictionary<string, BaseIni> Inis = new Dictionary<string, BaseIni>() {
            { BingIni.ID, new BingIni() },
            { NasaIni.ID, new NasaIni() },
            { OneplusIni.ID, new OneplusIni() },
            { TimelineIni.ID, new TimelineIni() },
            { YmyouliIni.ID, new YmyouliIni() },
            { InfinityIni.ID, new InfinityIni() },
            { G3Ini.ID, new G3Ini() },
            { PixivelIni.ID, new PixivelIni() },
            { LofterIni.ID, new LofterIni() },
            { DaihanIni.ID, new DaihanIni() },
            { DmoeIni.ID, new DmoeIni() },
            { ToubiecIni.ID, new ToubiecIni() },
            { MtyIni.ID, new MtyIni() },
            { SeovxIni.ID, new SeovxIni() },
            //{ MxgIni.ID, new MxgIni() },
            { PaulIni.ID, new PaulIni() }
        };

        public static HashSet<string> PUSH = new HashSet<string>() { "", "desktop", "lock" };
        public static HashSet<string> THEME = new HashSet<string>() { "", "light", "dark" };

        private string provider = BingIni.ID;
        public string Provider {
            set => provider = Inis.ContainsKey(value) ? value : BingIni.ID;
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

        private string theme = "";
        public string Theme {
            set => theme = THEME.Contains(value) ? value : "";
            get => theme;
        }

        public BaseIni GetIni(string provider = null) {
            return provider != null && Inis.ContainsKey(provider)
                ? Inis[provider] : Inis[this.provider];
        }

        public BaseProvider GenerateProvider(string provider = null) {
            return provider != null && Inis.ContainsKey(provider)
                ? Inis[provider].GenerateProvider() : Inis[this.provider].GenerateProvider();
        }

        override public string ToString() {
            string paras = Inis[provider].ToString();
            return $"/{provider}?push={push}&period={period}" + (paras.Length > 0 ? "&" : "") + paras;
        }
    }

    public class BaseIni {
        public string Id { set; get; }

        // 时序图源
        public virtual bool IsSequential() => true;

        public virtual BaseProvider GenerateProvider() => new BingProvider();
    }

    public class BingIni : BaseIni {
        public const string ID = "bing";
        public static HashSet<string> LANG = new HashSet<string>() { "", "zh-cn", "en-us", "ja-jp", "de-de", "fr-fr" };

        private string lang = "";
        public string Lang {
            set => lang = LANG.Contains(value) ? value : "";
            get => lang;
        }

        public BingIni() => Id = ID;

        public override BaseProvider GenerateProvider() => new BingProvider() { Id = ID };

        override public string ToString() => $"lang={lang}";
    }

    public class NasaIni : BaseIni {
        public const string ID = "nasa";
        public static HashSet<string> MIRROR = new HashSet<string>() { "", "bjp" };

        private string mirror = "";
        public string Mirror {
            set => mirror = MIRROR.Contains(value) ? value : "";
            get => mirror;
        }

        public NasaIni() => Id = ID;

        public override BaseProvider GenerateProvider() {
            switch (mirror) {
                case "bjp":
                    return new NasabjpProvider() { Id = ID };
                default:
                    return new NasaProvider() { Id = ID };
            }
        }

        override public string ToString() => $"mirror={mirror}";
    }

    public class OneplusIni : BaseIni {
        public const string ID = "oneplus";
        public static HashSet<string> ORDER = new HashSet<string>() { "date", "rate", "view" };

        private string order = "date";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "date";
            get => order;
        }

        public OneplusIni() => Id = ID;

        public override bool IsSequential() => "date".Equals(order);

        public override BaseProvider GenerateProvider() => new OneplusProvider() { Id = ID };

        override public string ToString() => $"order={order}";
    }

    public class TimelineIni : BaseIni {
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

        public TimelineIni() => Id = ID;

        public override bool IsSequential() => "date".Equals(order);

        public override BaseProvider GenerateProvider() => new TimelineProvider() { Id = ID };

        override public string ToString() => $"order={order}&cate={cate}";
    }

    public class YmyouliIni : BaseIni {
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

        public YmyouliIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new YmyouliProvider() { Id = ID };

        override public string ToString() => $"col={col}";
    }

    public class InfinityIni : BaseIni {
        public const string ID = "infinity";
        public static HashSet<string> ORDER = new HashSet<string>() { "", "rate" };

        private string order = "";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "";
            get => order;
        }

        public InfinityIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new InfinityProvider() { Id = ID };

        override public string ToString() => $"order={order}";
    }

    public class G3Ini : BaseIni {
        public const string ID = "3g";
        public static HashSet<string> ORDER = new HashSet<string>() { "date", "view" };

        private string order = "date";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "date";
            get => order;
        }

        public G3Ini() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new G3Provider() { Id = ID };

        override public string ToString() => $"order={order}";
    }

    public class PixivelIni : BaseIni {
        public const string ID = "pixivel";

        private int sanity = 5;
        public int Sanity {
            set => sanity = value > 0 ? value : 1;
            get => sanity;
        }

        public PixivelIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new PixivelProvider() { Id = ID };

        override public string ToString() => $"sanity={sanity}";
    }

    public class LofterIni : BaseIni {
        public const string ID = "lofter";

        public LofterIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new LofterProvider() { Id = ID };

        override public string ToString() => "";
    }

    public class DaihanIni : BaseIni {
        public const string ID = "daihan";

        public DaihanIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new DaihanProvider() { Id = ID };

        override public string ToString() => "";
    }

    public class DmoeIni : BaseIni {
        public const string ID = "dmoe";

        public DmoeIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new DmoeProvider() { Id = ID };

        override public string ToString() => "";
    }

    public class ToubiecIni : BaseIni {
        public const string ID = "toubiec";

        public ToubiecIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new ToubiecProvider() { Id = ID };

        override public string ToString() => "";
    }

    public class MtyIni : BaseIni {
        public const string ID = "mty";

        public MtyIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new MtyProvider() { Id = ID };

        override public string ToString() => "";
    }

    public class SeovxIni : BaseIni {
        public const string ID = "seovx";
        public static HashSet<string> CATE = new HashSet<string>() { "", "d", "ha" };

        private string cate = "d";
        public string Cate {
            set => cate = CATE.Contains(value) ? value : "d";
            get => cate;
        }

        public SeovxIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new SeovxProvider() { Id = ID };

        override public string ToString() => $"cate={cate}";
    }

    // deprecated: 开始收费
    public class MxgIni : BaseIni {
        public const string ID = "muxiaoguo";
        public static HashSet<string> CATE = new HashSet<string>() { "sjbz", "acg", "meinvtu" };

        private string cate = "sjbz";
        public string Cate {
            set => cate = CATE.Contains(value) ? value : "dsjbz";
            get => cate;
        }

        public MxgIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() {
            switch (cate) {
                case "acg":
                    return new MxgAcgProvider() { Id = ID };
                case "meinvtu":
                    return new MxgMvProvider() { Id = ID };
                default:
                    return new MxgProvider() { Id = ID };
            }
        }

        override public string ToString() => $"cate={cate}";
    }

    public class PaulIni : BaseIni {
        public const string ID = "paul";

        public PaulIni() => Id = ID;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new PaulProvider() { Id = ID };

        override public string ToString() => "";
    }
}
