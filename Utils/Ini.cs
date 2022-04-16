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

        private readonly HashSet<string> THEME = new HashSet<string>() { "", "light", "dark" };

        private string provider = BingIni.ID;
        public string Provider {
            set => provider = Inis.ContainsKey(value) ? value : BingIni.ID;
            get => provider;
        }

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
        public static readonly List<string> ORDER = new List<string>() { "date", "score", "random" };
        public static readonly List<string> CATE = new List<string>() { "", "acgcharacter", "acgscene", "sky",
            "war", "sword", "artistry", "car", "portrait", "animal", "delicacy", "nature" };

        private string order = "random";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "random";
            get => order;
        }

        private string cate = "";
        public string Cate {
            set => cate = CATE.Contains(value) ? value : "";
            get => cate;
        }

        public int Qc { set; get; } = 1;

        public override bool IsSequential() => false;

        public override BaseProvider GenerateProvider() => new YmyouliProvider() { Id = ID };

        override public string ToString() => $"desktopperiod={DesktopPeriod}&lockperiod={LockPeriod}&order={Order}&cate={Cate}&qc={Qc}";
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
