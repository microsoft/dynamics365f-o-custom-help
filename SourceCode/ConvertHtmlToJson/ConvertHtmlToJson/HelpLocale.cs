using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertHtmlToJson
{
    class HelpLocale
    {
        private CultureInfo helpCultureInfo;
        private Dictionary<CultureInfo, CultureInfo> gitHubLocaleToFOLocale =
                            new Dictionary<CultureInfo, CultureInfo>()
                            {
                                { new CultureInfo("ar-sa"), new CultureInfo ("ar") },
                                { new CultureInfo("cs-cz"), new CultureInfo ("cs") },
                                { new CultureInfo("da-dk"), new CultureInfo ("da") },
                                { new CultureInfo("de-at"), new CultureInfo ("de-at") },
                                { new CultureInfo("de-ch"), new CultureInfo ("de-ch") },
                                { new CultureInfo("de-de"), new CultureInfo ("de") },
                                { new CultureInfo("en-au"), new CultureInfo ("en-au") },
                                { new CultureInfo("en-ca"), new CultureInfo ("en-ca") },
                                { new CultureInfo("en-gb"), new CultureInfo ("en-gb") },
                                { new CultureInfo("en-ie"), new CultureInfo ("en-ie") },
                                { new CultureInfo("en-in"), new CultureInfo ("en-in") },
                                { new CultureInfo("en-my"), new CultureInfo ("en-my") },
                                { new CultureInfo("en-nz"), new CultureInfo ("en-nz") },
                                { new CultureInfo("en-sg"), new CultureInfo ("en-sg") },
                                { new CultureInfo("en-us"), new CultureInfo ("en-us") },
                                { new CultureInfo("en-za"), new CultureInfo ("en-za") },
                                { new CultureInfo("es-es"), new CultureInfo ("es") },
                                { new CultureInfo("es-mx"), new CultureInfo ("es-mx") },
                                { new CultureInfo("et-ee"), new CultureInfo ("et") },
                                { new CultureInfo("fi-fi"), new CultureInfo ("fi") },
                                { new CultureInfo("fr-be"), new CultureInfo ("fr-be") },
                                { new CultureInfo("fr-ca"), new CultureInfo ("fr-ca") },
                                { new CultureInfo("fr-ch"), new CultureInfo ("fr-ch") },
                                { new CultureInfo("fr-fr"), new CultureInfo ("fr") },
                                { new CultureInfo("hu-hu"), new CultureInfo ("hu") },
                                { new CultureInfo("is-is"), new CultureInfo ("is") },
                                { new CultureInfo("it-ch"), new CultureInfo ("it-ch") },
                                { new CultureInfo("it-it"), new CultureInfo ("it") },
                                { new CultureInfo("ja-jp"), new CultureInfo ("ja") },
                                { new CultureInfo("lt-lt"), new CultureInfo ("lt") },
                                { new CultureInfo("lv-lv"), new CultureInfo ("lv") },
                                { new CultureInfo("nb-no"), new CultureInfo ("nb-no") },
                                { new CultureInfo("nl-be"), new CultureInfo ("nl-be") },
                                { new CultureInfo("nl-nl"), new CultureInfo ("nl") },
                                { new CultureInfo("pl-pl"), new CultureInfo ("pl-pl") },
                                { new CultureInfo("pt-br"), new CultureInfo ("pt-br") },
                                { new CultureInfo("ru-ru"), new CultureInfo ("ru-ru") },
                                { new CultureInfo("sv-se"), new CultureInfo ("sv") },
                                { new CultureInfo("th-th"), new CultureInfo ("th") },
                                { new CultureInfo("tr-tr"), new CultureInfo ("tr") },
                                { new CultureInfo("zh-cn"), new CultureInfo ("zh-hans") }
                            };

        public HelpLocale(String locale)
        {
            this.helpCultureInfo = new CultureInfo(locale);
            gitHubLocaleToFOLocale.TryGetValue(this.helpCultureInfo, out this.helpCultureInfo);
        }

        public CultureInfo GetCultureInfo()
        {
            return this.helpCultureInfo;
        }
    }
}
