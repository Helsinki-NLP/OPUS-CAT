using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    /// <summary>
    /// This is a custom language class needed for dealing with the wide variety of languages
    /// in Opus and Tatoeba Challenge. Some of the language codes are two-letter, some three-letter, and
    /// most don't have CultureInfos available.
    /// </summary>
    public class IsoLanguage
    {

        private static void ParseIso639_3()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var isoTable = new StreamReader(assembly.GetManifestResourceStream("OpusCatMTEngine.iso-639-3_20200515.tab")))
            {
                //Skip header
                isoTable.ReadLine();

                string line;
                while ((line = isoTable.ReadLine()) != null)
                {
                    var split = line.Split('\t');
                    var iso639_3 = split[0];
                    
                    var refname = split[6];
                    IsoLanguage.Iso639_3ToRefName[iso639_3] = refname;

                    //Also record the bibliographical iso639-2 codes, they
                    //are used in e.g. memoQ
                    string iso639_2b = null;
                    if (split[1].Length == 3 && split[1] != iso639_3)
                    {
                        iso639_2b = split[1];
                        IsoLanguage.Iso639_2BTo639_3[iso639_2b] = iso639_3;
                    }

                    //Some languages have ISO 639 1 two-letter codes
                    if (split[3].Length == 2)
                    {
                        var iso639_1 = split[3];
                        IsoLanguage.Iso639_1To639_3[iso639_1] = iso639_3;
                        IsoLanguage.Iso639_3To639_1[iso639_3] = iso639_1;
                    }



                }
            }
        }

        private static void ParseIso639_5()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var isoTable = new StreamReader(assembly.GetManifestResourceStream("OpusCatMTEngine.iso639-5.tsv")))
            {
                //Skip header
                isoTable.ReadLine();

                string line;
                while ((line = isoTable.ReadLine()) != null)
                {
                    var split = line.Split('\t');
                    var iso639_5 = split[1];
                    var refname = split[2];
                    IsoLanguage.Iso639_5ToRefName[iso639_5] = refname;
                    
                }
            }
        }

            //this static constructor parses the iso table files that are embedded as resources
        static IsoLanguage()
        {
            IsoLanguage.ParseIso639_3();
            IsoLanguage.ParseIso639_5();
            
        }

        private static Dictionary<string, string> Iso639_3To639_1 = new Dictionary<string, string>();
        private static Dictionary<string, string> Iso639_1To639_3 = new Dictionary<string, string>();
        private static Dictionary<string, string> Iso639_2BTo639_3 = new Dictionary<string, string>();
        private static Dictionary<string, string> Iso639_3ToRefName = new Dictionary<string, string>();
        private static Dictionary<string, string> Iso639_5ToRefName = new Dictionary<string, string>();

        public string Iso639_5Code { get; set; }
        public string Iso3166Locale { get; set; }
        public string Iso639_3Code { get; set; }
        public string Iso639_1Code { get; set; }
        public string NonIsoCode { get; set; }

        public override string ToString()
        {
            return this.ShortestIsoCode;
        }

        public string ShortestIsoCode
        {
            get
            {
                if (this.Iso639_1Code != null && this.Iso639_1Code.Length == 2)
                {
                    return this.Iso639_1Code;
                }
                else if (this.Iso639_3Code != null)
                {
                    return this.Iso639_3Code;
                }
                else if (this.Iso639_5Code != null)
                {
                    return this.Iso639_5Code;
                }
                else
                {
                    return this.NonIsoCode;
                }
            }
        }

        public string Iso15924Script { get; set; }
        public string IsoRefName { get; }
        public string OriginalCode { get; }

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

            //Store the original code. This is needed for e.g. picking the correct source file code for
            //tokenization and correct target language code for multilingual models
            this.OriginalCode = languageCode;

            //Format checking
            Match opusMatch = IsoLanguage.OpusMtCode.Match(languageCode);
            if (opusMatch != null)
            {
                this.Iso639_1Code = opusMatch.Groups["iso639_1"].Value;

                var threeLetterIsoCode = opusMatch.Groups["iso639_3"].Value;

                //the code may be iso 639 2B code, in which case convert it here to iso 639 3
                if (IsoLanguage.Iso639_2BTo639_3.ContainsKey(threeLetterIsoCode))
                {
                    threeLetterIsoCode = IsoLanguage.Iso639_2BTo639_3[threeLetterIsoCode];
                }

                if (IsoLanguage.Iso639_3ToRefName.ContainsKey(threeLetterIsoCode))
                {
                    this.Iso639_3Code = threeLetterIsoCode;
                }
                else if (IsoLanguage.Iso639_5ToRefName.ContainsKey(threeLetterIsoCode))
                {
                    this.Iso639_5Code = threeLetterIsoCode;
                }

                this.Iso3166Locale = opusMatch.Groups["locale"].Value;
            }
            else
            {
                Match cultureInfoMatch = IsoLanguage.CultureInfoCode.Match(languageCode);
                if (cultureInfoMatch != null)
                {
                    this.Iso639_1Code = cultureInfoMatch.Groups["iso639_1"].Value;
                    this.Iso639_3Code = cultureInfoMatch.Groups["iso639_3"].Value;
                    this.Iso3166Locale = cultureInfoMatch.Groups["locale"].Value;
                    this.Iso15924Script = cultureInfoMatch.Groups["script"].Value;
                }
            }

            //If parsing has failed, store the language code as non iso
            if ((String.IsNullOrEmpty(this.Iso639_1Code) || !IsoLanguage.Iso639_1To639_3.ContainsKey(this.Iso639_1Code)) &&
                (String.IsNullOrEmpty(this.Iso639_3Code) || !IsoLanguage.Iso639_3To639_1.ContainsKey(this.Iso639_3Code)) &&
                (String.IsNullOrEmpty(this.Iso639_5Code) || !IsoLanguage.Iso639_5ToRefName.ContainsKey(this.Iso639_5Code)))
            {
                this.NonIsoCode = languageCode;
            }
            else
            {
                //Get iso639_1 from is639_3 code if one exists
                if (String.IsNullOrEmpty(Iso639_1Code) &&
                    !String.IsNullOrEmpty(this.Iso639_3Code) &&
                    IsoLanguage.Iso639_3To639_1.ContainsKey(this.Iso639_3Code))
                {
                    this.Iso639_1Code = IsoLanguage.Iso639_3To639_1[this.Iso639_3Code];
                }
                else if (!String.IsNullOrEmpty(this.Iso639_1Code) && (String.IsNullOrEmpty(this.Iso639_3Code)))
                {
                    this.Iso639_3Code = IsoLanguage.Iso639_1To639_3[this.Iso639_1Code];
                }

                if (this.Iso639_3Code != null)
                {
                    this.IsoRefName = IsoLanguage.Iso639_3ToRefName[this.Iso639_3Code];
                }
                else
                {
                    this.IsoRefName = IsoLanguage.Iso639_5ToRefName[this.Iso639_5Code];
                }
            }
        }


        internal bool IsCompatibleLanguage(IsoLanguage lang)
        {
            //TODO: add language group compatibility matching here
            return this.ShortestIsoCode == lang.ShortestIsoCode;
        }

        //OPUS MT code is usually ISO-639-1, but might be ISO-639-3
        //In multilingual codes the separator is +, and there might be a locale specifier with underscore,
        //e.g. pt_br
        private static Regex OpusMtCode = new Regex(@"^((?<iso639_1>\w{2})|(?<iso639_3>\w{3}))(_(?<locale>\w{2}))?$");

        //Trados returns CultureInfo strings. They could be parsed just by using the CultureInfo constructor, but
        //unfortunately CultureInfo has no explicit property for writing system, so might as well parse them with this.
        private static Regex CultureInfoCode = new Regex(@"(^(?<iso639_1>\w{2})|(?<iso639_3>\w{3}))(-(?<script>\w{4}))?(-(?<locale>\w{2}))?$");


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
