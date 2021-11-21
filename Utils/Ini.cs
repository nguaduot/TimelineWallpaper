using System.Collections.Generic;
using TimelineWallpaper.Providers;

namespace TimelineWallpaper.Utils {
    public class Ini {
        public static HashSet<string> PROVIDER = new HashSet<string>() {
            "bing", "nasa", "oneplus", "timeline",
            "ymyouli", "infinity", "3g", "pixivel", "lofter",
            "daihan", "dmoe", "toubiec", "mty", "seovx", "muxiaoguo", "paul"
        };
        public static HashSet<string> PUSH = new HashSet<string>() { "", "desktop", "lock" };

        private string provider = "bing";
        public string Provider {
            set => provider = PROVIDER.Contains(value) ? value : "bing";
            get => provider;
        }

        private string push = "";
        public string Push {
            set => push = PUSH.Contains(value) ? value : "";
            get => push;
        }

        private int period = 24;
        public int Period {
            set => period = period <= 0 || period > 24 ? 24 : period;
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
                case "nasa":
                    if ("bjp".Equals(Nasa.Mirror)) {
                        return new NasabjpProvider();
                    }
                    return new NasaProvider();
                case "oneplus":
                    return new OneplusProvider();
                case "timeline":
                    return new TimelineProvider();
                case "ymyouli":
                    return new YmyouliProvider();
                case "infinity":
                    return new InfinityProvider();
                case "3g":
                    return new G3Provider();
                case "pixivel":
                    return new PixivelProvider();
                case "lofter":
                    return new LofterProvider();
                case "daihan":
                    return new DaihanProvider();
                case "dmoe":
                    return new DmoeProvider();
                case "toubiec":
                    return new ToubiecProvider();
                case "mty":
                    return new MtyProvider();
                case "seovx":
                    return new SeovxProvider();
                case "muxiaoguo":
                    if ("acg".Equals(Mxg.Cate)) {
                        return new MxgAcgProvider();
                    } else if ("meinvtu".Equals(Mxg.Cate)) {
                        return new MxgMvProvider();
                    }
                    return new MxgProvider();
                case "paul":
                    return new PaulProvider();
                case "bing":
                default:
                    return new BingProvider();
            }
        }
    }

    public class ProviderBing {
        public static HashSet<string> LANG = new HashSet<string>() { "", "zh-cn", "en-us", "ja-jp", "de-de", "fr-fr" };

        private string lang = "";
        public string Lang {
            set => lang = LANG.Contains(value) ? value : "";
            get => lang;
        }
    }

    public class ProviderNasa {
        public static HashSet<string> MIRROR = new HashSet<string>() { "", "bjp" };

        private string mirror = "";
        public string Mirror {
            set => mirror = MIRROR.Contains(value) ? value : "";
            get => mirror;
        }
    }

    public class ProviderOnePlus {
        public static HashSet<string> ORDER = new HashSet<string>() { "date", "rate", "view" };

        private string order = "date";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "date";
            get => order;
        }
    }

    public class ProviderTimeline {
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
    }

    public class ProviderYmyouli {
        private string col = "";
        public string Col {
            set => col = YmyouliProvider.COL_MODULE_DIC.ContainsKey(value) ? value : "";
            get => col;
        }
    }

    public class ProviderInfinity {
        public static HashSet<string> ORDER = new HashSet<string>() { "", "rate" };

        private string order = "";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "";
            get => order;
        }
    }

    public class Provider3G {
        public static HashSet<string> ORDER = new HashSet<string>() { "date", "view" };

        private string order = "date";
        public string Order {
            set => order = ORDER.Contains(value) ? value : "date";
            get => order;
        }
    }

    public class ProviderPixivel {
        private int sanity = 5;
        public int Sanity {
            set => sanity = value > 0 ? value : 1;
            get => sanity;
        }
    }

    public class ProviderLofter { }

    public class ProviderDaihan { }

    public class ProviderDmoe { }

    public class ProviderToubiec { }

    public class ProviderMty { }

    public class ProviderSeovx {
        public static HashSet<string> CATE = new HashSet<string>() { "", "d", "ha" };

        private string cate = "d";
        public string Cate {
            set => cate = CATE.Contains(value) ? value : "d";
            get => cate;
        }
    }

    public class ProviderMxg {
        public static HashSet<string> CATE = new HashSet<string>() { "sjbz", "acg", "meinvtu" };

        private string cate = "sjbz";
        public string Cate {
            set => cate = CATE.Contains(value) ? value : "dsjbz";
            get => cate;
        }
    }

    public class ProviderPaul { }
}
