﻿using System.Collections.Generic;
using TimelineWallpaper.Providers;

namespace TimelineWallpaper.Utils {
    public class Ini {
        public static HashSet<string> PROVIDER = new HashSet<string>() {
            "bing", "nasa", "oneplus",
            "lofter", "3g", "pixivel", "ymyouli", "infinity",
            "daihan", "paul"
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

        public ProviderLofter Lofter { set; get; } = new ProviderLofter();

        public Provider3G G3 { set; get; } = new Provider3G();

        public ProviderPixivel Pixivel { set; get; } = new ProviderPixivel();

        public ProviderYmyouli Ymyouli { set; get; } = new ProviderYmyouli();

        public ProviderInfinity Infinity { set; get; } = new ProviderInfinity();

        public ProviderDaihan Daihan { set; get; } = new ProviderDaihan();

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
                case "lofter":
                    return new LofterProvider();
                case "3g":
                    return new G3Provider();
                case "pixivel":
                    return new PixivelProvider();
                case "ymyouli":
                    return new YmyouliProvider();
                case "infinity":
                    return new InfinityProvider();
                case "daihan":
                    return new DaihanProvider();
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

    public class ProviderLofter { }

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

    public class ProviderDaihan { }

    public class ProviderPaul { }
}
