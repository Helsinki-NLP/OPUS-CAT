using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FiskmoMTEngine
{
    /// <summary>
    /// This is a custom language class needed for dealing with the wide variety of languages
    /// in Opus and Tatoeba Challenge. Some of the language codes are two-letter, some three-letter, and
    /// most don't have CultureInfos available.
    /// </summary>
    public class IsoLanguage
    {
        public string Iso639_5Code { get; set; }
        public string Iso3166Locale { get; set; }
        public string Iso639_3Code { get; set; }
        public string Iso639_1Code { get; set; }

        public string Iso15924Script { get; set; }

        //This constructor parses the code.
        //The language code may be from a MT model, Opus MT models generally have ISO-639-1 codes if possible,
        //otherwise ISO-639-3 (this is the case with most low-resource languages). Tatoeba models use ISO-639-3
        //for all models, and also ISO-639-5 language family codes for multilingual models. Script may be indicated by
        //ISO 15924 code
        //Multilingual models in OPUT-MT contain all the languages separated by plus signs. The languages may also contain
        //country codes there.
        //The language code may also be from a CAT tool, in which case it usually also contains a country code,
        //like "en-GB", "sv-FI", "sv-SV".
        //The purpose of this class is to convert all these dissimilar formats to objects of one type.
        public IsoLanguage(string languageCode)
        {
            //Format checking
            Match opusMatch = IsoLanguage.OpusMtCode.Match(languageCode);
            if (opusMatch != null)
            {
                this.Iso639_1Code = opusMatch.Groups["iso_639_1"].Value;
                this.Iso639_3Code = opusMatch.Groups["iso_639_3"].Value;
                this.Iso3166Locale = opusMatch.Groups["locale"].Value;
            }

            Match cultureInfoMatch = IsoLanguage.CultureInfoCode.Match(languageCode);
            if (cultureInfoMatch != null)
            {
                this.Iso639_1Code = cultureInfoMatch.Groups["iso_639_1"].Value;
                this.Iso639_3Code = cultureInfoMatch.Groups["iso_639_3"].Value;
                this.Iso3166Locale = cultureInfoMatch.Groups["locale"].Value;
                this.Iso15924Script = cultureInfoMatch.Groups["script"].Value;
            }
        }


        internal bool IsCompatibleLanguage(IsoLanguage lang)
        {
            return this.Iso639_3Code == lang.Iso639_3Code;
        }

        //OPUS MT code is usually ISO-639-1, but might be ISO-639-3
        //In multilingual codes the separator is +, and there might be a locale specifier with underscore,
        //e.g. pt_br
        private static Regex OpusMtCode = new Regex(@"((?<iso639_1>\w{2})|(?<iso639_3>\w{3}))(_(?<locale>\w{2}))?");

        //Trados returns CultureInfo strings. They could be parsed just by using the CultureInfo constructor, but
        //unfortunately CultureInfo has no explicit property for writing system, so might as well parse them with this.
        private static Regex CultureInfoCode = new Regex(@"((?<iso639_1>\w{2})|(?<iso639_3>\w{3}))(-(?<script>\w{4}))?(-(?<locale>\w{2}))?");


        //This is used with tmx's to filter relevant segments
        //xml:lang codes are supposed to be from here: https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry
        //Older tmx's might have idiosyncratic codes.
        //The basic principle should be that the first part of the code is ISO639-1, if it exists,
        //otherwise ISO639-3
        internal bool IsCompatibleTmxLang(string xmlLangCode)
        {
            //code might contain locale after hyphen
            var languagePart = xmlLangCode.Split('-')[0];

            return (languagePart == this.Iso639_1Code || languagePart == this.Iso639_3Code);
        }
    }
}
