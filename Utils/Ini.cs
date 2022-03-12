using System.Collections.Generic;
using TimelineWallpaper.Providers;

namespace TimelineWallpaper.Utils {
    public class Ini {
        private readonly Dictionary<string, BaseIni> Inis = new Dictionary<string, BaseIni>() {
            { BingIni.ID, new BingIni() },
            { NasaIni.ID, new NasaIni() },
            { OneplusIni.ID, new OneplusIni() },
            { TimelineIni.ID, new TimelineIni() },
            { Himawari8Ini.ID, new Himawari8Ini() },
            { YmyouliIni.ID, new YmyouliIni() },
            { InfinityIni.ID, new InfinityIni() },
            { OneIni.ID, new OneIni() },
            { G3Ini.ID, new G3Ini() },
            { BoboIni.ID, new BoboIni() },
            { LofterIni.ID, new LofterIni() },
            { AbyssIni.ID, new AbyssIni() },
            { DaihanIni.ID, new DaihanIni() },
            { DmoeIni.ID, new DmoeIni() },
            { ToubiecIni.ID, new ToubiecIni() },
            { MtyIni.ID, new MtyIni() },
            { SeovxIni.ID, new SeovxIni() },
            { PaulIni.ID, new PaulIni() }
        };

        //private readonly HashSet<string> PUSH = new HashSet<string>() { "", "desktop", "lock" };
        private readonly HashSet<string> THEME = new HashSet<string>() { "", "light", "dark" };

        private string provider = BingIni.ID;
        public string Provider {
            set => provider = Inis.ContainsKey(value) ? value : BingIni.ID;
            get => provider;
        }

        //private string push = "";
        //public string Push {
        //    set => push = PUSH.Contains(value) ? value : "";
        //    get => push;
        //}

        //private string pushProvider = BingIni.ID;
        //public string PushProvider {
        //    set => pushProvider = Inis.ContainsKey(value) ? value : BingIni.ID;
        //    get => pushProvider;
        //}

        private string desktopProvider = "";
        public string DesktopProvider {
            set => desktopProvider = Inis.ContainsKey(value) ? value : "";
            get => desktopProvider;
        }

        private string lockProvider = "";
        public string LockProvider {
            set => lockProvider = Inis.ContainsKey(value) ? value : "";
            get => lockProvider;
        }

        private string theme = "";
        public string Theme {
            set => theme = THEME.Contains(value) ? value : "";
            get => theme;
        }

        public bool SetIni(string provider, BaseIni ini) {
            if (Inis.ContainsKey(provider) && ini != null) {
                Inis[provider] = ini;
                return true;
            }
            return false;
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
            return $"/{Provider}?desktopprovider={DesktopProvider}&lockprovider={LockProvider}" + (paras.Length > 0 ? "&" : "") + paras;
        }
    }

    public class BaseIni {
        private int desktopPeriod = 24;
        public int DesktopPeriod {
            set => desktopPeriod = value <= 0 || value > 24 ? 24 : value;
            get => desktopPeriod;
        }

        private int lockPeriod = 24;
        public int LockPeriod {
            set => lockPeriod = value <= 0 || value > 24 ? 24 : value;
            get => lockPeriod;
        }

        // 时序图源
        public virtual bool IsSequential() => true;

        public virtual BaseProvider GenerateProvider() => new BingProvider();
    }

    public class BingIni : BaseIni {
        public const string ID = "bing";
        public static readonly List<string> LANG = new List<string>() { "", "zh-cn", "en-us", "ja-jp", "de-de", "fr-fr" };

        private string lang = "";
        public string Lang {
            set => lang = LANG.Contains(value) ? value : "";
            get => lang;
        }

        public override BaseProvider GenerateProvider() => new BingProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}&lang={Lang}";
    }

    public class NasaIni : BaseIni {
        public const string ID = "nasa";
        public static readonly List<string> MIRROR = new List<string>() { "", "bjp" };

        private string mirror = "";
        public string Mirror {
            set => mirror = MIRROR.Contains(value) ? value : "";
            get => mirror;
        }

        public override BaseProvider GenerateProvider() {
            switch (mirror) {
                case "bjp":
                    return new NasabjpProvider() { Id = ID };
                default:
                    return new NasaProvider() { Id = ID };
            }
        }

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}&mirror={Mirror}";
    }

    public class OneplusIni : BaseIni {
        public const string ID = "oneplus";
        public static readonly List<string> ORDER = new List<string>() { "date", "rate", "view" };

        private string order = "date";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "date";
            get => order;
        }

        public override bool IsSequential() => "date".Equals(order);

        public override BaseProvider GenerateProvider() => new OneplusProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}&order={Order}";
    }

    public class TimelineIni : BaseIni {
        public const string ID = "timeline";
        public static readonly List<string> ORDER = new List<string>() { "date", "random" };
        public static readonly List<string> CATE = new List<string>() { "", "landscape", "portrait", "culture", "term" };

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

        public int Authorize { set; get; } = 1;

        public override bool IsSequential() => "date".Equals(order);

        public override BaseProvider GenerateProvider() => new TimelineProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}&order={Order}&cate={Cate}&authorize={Authorize}";
    }

    public class Himawari8Ini : BaseIni {
        public const string ID = "himawari8";
        
        private float offset = 0;
        public float Offset {
            set => offset = value < -1 ? -1 : (value > 1 ? 1 : value);
            get => offset;
        }

        public Himawari8Ini() {
            DesktopPeriod = 1;
            LockPeriod = 2;
        }

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new Himawari8Provider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}";
    }

    public class YmyouliIni : BaseIni {
        public const string ID = "ymyouli";
        public static readonly SortedDictionary<string, Dictionary<string, string>> COL_MODULE_DIC = new SortedDictionary<string, Dictionary<string, string>> {
            { "182", new Dictionary<string, string> {
                { "577", "126" },
                { "606", "126" },
                { "607", "126" },
                { "611", "126" },
                { "681", "126" }, // 4K-第五栏
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
                { "842", "182" },
                { "834", "182" },
                { "844", "182" },
                { "845", "182" }, // 8K-第一章-第十五栏
                { "848", "248" } // 8K-第二章-第十六栏
            } }, // 游戏动漫人物（4K+8K）
            { "183", new Dictionary<string, string> {
                { "677", "127" }, // 4K-第一栏
                { "673", "183" },
                { "777", "183" } // 8K-第一章-第二栏
            } }, // 游戏动漫场景（4K+8K）
            { "184", new Dictionary<string, string> {
                { "678", "134" }, // 4K-第一栏
                { "675", "184" },
                { "791", "184" } // 8K-第一章-第二栏
            } }, // 自然风景（4K+8K）
            { "185", new Dictionary<string, string> {
                { "578", "185" },
                { "679", "185" },
                { "680", "185" },
                { "754", "185" } // 8K-第一章-第四栏
            } }, // 花草植物
            { "186", new Dictionary<string, string> {
                { "753", "186" } // 8K-第一章-第一栏
            } }, // 美女女孩
            { "187", new Dictionary<string, string> {
                { "670", "187" },
                { "741", "187" },
                { "790", "187" } // 8K-第一章-第三栏
            } }, // 机车
            { "214", new Dictionary<string, string> {
                { "690", "214" },
                { "691", "214" } // 8K-第一章-第二栏
            } }, // 科幻
            { "215", new Dictionary<string, string> {
                { "693", "215" },
                { "694", "215" },
                { "742", "215" },
                { "836", "215" } // 8K-第一章-第四栏
            } }, // 意境
            { "224", new Dictionary<string, string> {
                { "746", "224" } // 8K-第一章-第一栏
            } }, // 武器刀剑
            { "225", new Dictionary<string, string> {
                { "748", "225" } // 8K-第一章-第一栏
            } }, // 动物
            { "226", new Dictionary<string, string> {
                { "682", "128" }, // 4K-第一栏
                { "751", "226" } // 8K-第一章-第一栏
            } }, // 古风人物（4K+8K）
            { "227", new Dictionary<string, string> {
                { "756", "227" },
                { "773", "227" } // 8K-第一章-第二栏
            } }, // 日暮云天
            { "228", new Dictionary<string, string> {
                { "758", "228" } // 8K-第一章-第一栏
            } }, // 夜空星河
            { "229", new Dictionary<string, string> {
                { "760", "229" },
                { "761", "229" },
                { "762", "229" },
                { "843", "229" } // 8K-第一章-第四栏
            } }, // 战场战争
            { "230", new Dictionary<string, string> {
                { "763", "230" } // 8K-第一章-第一栏
            } }, // 冰雪之境
            { "231", new Dictionary<string, string> {
                { "766", "231" } // 8K-第一章-第一栏
            } }, // 油画
            { "232", new Dictionary<string, string> {
                { "775", "232" } // 8K-第一章-第一栏
            } }, // 国漫壁纸
            { "233", new Dictionary<string, string> {
                { "778", "233" } // 8K-第一章-第一栏
            } }, // 美食蔬果
            { "241", new Dictionary<string, string> {
                { "830", "241" } // 8K-第一章-第一栏
            } } // 樱落
        }; // { col: { module: col } }

        private string col = "";
        public string Col {
            set => col = COL_MODULE_DIC.ContainsKey(value) ? value : "";
            get => col;
        }

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new YmyouliProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}&col={Col}";
    }

    public class InfinityIni : BaseIni {
        public const string ID = "infinity";
        public static readonly List<string> ORDER = new List<string>() { "", "rate" };

        private string order = "";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "";
            get => order;
        }

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new InfinityProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}&order={Order}";
    }

    public class OneIni : BaseIni {
        public const string ID = "one";
        public static readonly List<string> ORDER = new List<string>() { "date", "random" };
        
        private string order = "date";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "date";
            get => order;
        }

        public override bool IsSequential() => "date".Equals(order);

        public override BaseProvider GenerateProvider() => new OneProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}&order={Order}";
    }

    public class G3Ini : BaseIni {
        public const string ID = "3g";
        private readonly HashSet<string> ORDER = new HashSet<string>() { "date", "view" };

        private string order = "date";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "date";
            get => order;
        }

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new G3Provider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}&order={Order}";
    }

    public class BoboIni : BaseIni {
        public const string ID = "bobo";

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new BoboProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}";
    }

    public class LofterIni : BaseIni {
        public const string ID = "lofter";

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new LofterProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}";
    }

    public class AbyssIni : BaseIni {
        public const string ID = "abyss";

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new AbyssProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}";
    }

    public class DaihanIni : BaseIni {
        public const string ID = "daihan";

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new DaihanProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}";
    }

    public class DmoeIni : BaseIni {
        public const string ID = "dmoe";

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new DmoeProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}";
    }

    public class ToubiecIni : BaseIni {
        public const string ID = "toubiec";

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new ToubiecProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}";
    }

    public class MtyIni : BaseIni {
        public const string ID = "mty";

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new MtyProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}";
    }

    public class SeovxIni : BaseIni {
        public const string ID = "seovx";
        private readonly HashSet<string> CATE = new HashSet<string>() { "", "d", "ha" };

        private string cate = "d";
        public string Cate {
            set => cate = CATE.Contains(value) ? value : "d";
            get => cate;
        }

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new SeovxProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}&cate={Cate}";
    }

    public class PaulIni : BaseIni {
        public const string ID = "paul";

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new PaulProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}";
    }
}
