using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    /// <summary>
    /// This class replicates preprocessing steps from three perl scripts within
    /// the Moses repo: normalize-punctuation.perl, replace-unicode-punctuation.perl and
    /// remove-non-printing-char.perl
    /// </summary>
    public static class MosesPreprocessor
    {

        static MosesPreprocessor()
        {

        }

        public static string PreprocessSpaces(string sourceSentence)
        {
            var withoutPeripheralSpaces = sourceSentence.Trim();
            var withoutAdjacentSpaces = Regex.Replace(withoutPeripheralSpaces, "  *", " ");
            return withoutAdjacentSpaces;
        }

        public static string RunMosesPreprocessing(string input, string language)
        {
            var inputBuilder = new StringBuilder(input);
            ReplaceUnicodePunctuation(inputBuilder);
            input = RemoveNonPrintingChar(inputBuilder.ToString());
            //input = NormalizePunctuation(input, language);
            return input;
        }

        public static void ReplaceUnicodePunctuation(StringBuilder input)
        {
            input.Replace("，", ",");
            input.Replace("。", ". ");
            input.Replace("、", ",");
            input.Replace("”", "\"");
            input.Replace("“", "\"");
            input.Replace("∶", ":");
            input.Replace("：", ":");
            input.Replace("？", "?");
            input.Replace("《", "\"");
            input.Replace("》", "\"");
            input.Replace("）", ")");
            input.Replace("！", "!");
            input.Replace("（", "(");
            input.Replace("；", ";");
            input.Replace("１", "\"");
            input.Replace("」", "\"");
            input.Replace("「", "\"");
            input.Replace("０", "0");
            input.Replace("３", "3");
            input.Replace("２", "2");
            input.Replace("５", "5");
            input.Replace("６", "6");
            input.Replace("９", "9");
            input.Replace("７", "7");
            input.Replace("８", "8");
            input.Replace("４", "4");
            input.Replace("． *", ". ");
            input.Replace("～", "~");
            input.Replace("’", "'");
            input.Replace("…", "...");
            input.Replace("━", "-");
            input.Replace("〈", "<");
            input.Replace("〉", ">");
            input.Replace("【", "[");
            input.Replace("】", "]");
            input.Replace("％", "%");

        }

        private static Regex NonPrintingCharRegex = new Regex(@"\p{C}", RegexOptions.Compiled);

        public static string RemoveNonPrintingChar(string input)
        {
            input = NonPrintingCharRegex.Replace(input, " ");
            return input;
        }

        private static List<Tuple<Regex, String>> PunctuationRegexes1 = new List<Tuple<Regex, string>>()
        {
            MosesPreprocessor.CreateRegexWithReplacement("\r",""),
            // remove extra spaces
            MosesPreprocessor.CreateRegexWithReplacement(@"\("," ("),
            MosesPreprocessor.CreateRegexWithReplacement(@"\)",") "),
            MosesPreprocessor.CreateRegexWithReplacement(@" +", " "),
            MosesPreprocessor.CreateRegexWithReplacement(@"\) ([\.\!\:\?\;\,])", ")$1"),
            MosesPreprocessor.CreateRegexWithReplacement(@"\( ",  "("),
            MosesPreprocessor.CreateRegexWithReplacement(@" \)",  ")"),
            MosesPreprocessor.CreateRegexWithReplacement(@"(\d) \%",  "$1%"),
            MosesPreprocessor.CreateRegexWithReplacement(@" :",  ":"),
            MosesPreprocessor.CreateRegexWithReplacement(@" ;",  ";")
        };

        private static List<Tuple<Regex, String>> PunctuationRegexes2 = new List<Tuple<Regex, string>>()
        {
            MosesPreprocessor.CreateRegexWithReplacement("\r",""),
            MosesPreprocessor.CreateRegexWithReplacement( @"„", "\""),
            MosesPreprocessor.CreateRegexWithReplacement(@"“","\""),
            MosesPreprocessor.CreateRegexWithReplacement(@"”", "\""),
            MosesPreprocessor.CreateRegexWithReplacement(@"–"," - "),
            MosesPreprocessor.CreateRegexWithReplacement(@"—", " - "),
            MosesPreprocessor.CreateRegexWithReplacement(@" +", " "),
            MosesPreprocessor.CreateRegexWithReplacement(@"´", "'"),
            MosesPreprocessor.CreateRegexWithReplacement(@"([A-Za-z])‘([A-Za-z])", @"$1\'$2"),
            MosesPreprocessor.CreateRegexWithReplacement(@"([A-Za-z])’([A-Za-z])", @"$1\'$2"),
            MosesPreprocessor.CreateRegexWithReplacement(@"‘", "\""),
            MosesPreprocessor.CreateRegexWithReplacement(@"‚","\""),
            MosesPreprocessor.CreateRegexWithReplacement(@"’", "\""),
            MosesPreprocessor.CreateRegexWithReplacement(@"''","\""),
            MosesPreprocessor.CreateRegexWithReplacement(@"´´", "\""),
            MosesPreprocessor.CreateRegexWithReplacement(@"…",@"..."),
            // French quotes
            MosesPreprocessor.CreateRegexWithReplacement(@" « ", " \""),
            MosesPreprocessor.CreateRegexWithReplacement(@"« ","\""),
            MosesPreprocessor.CreateRegexWithReplacement(@"«", "\""),
            MosesPreprocessor.CreateRegexWithReplacement(@" » ","\" "),
            MosesPreprocessor.CreateRegexWithReplacement(@" »", "\""),
            MosesPreprocessor.CreateRegexWithReplacement(@"»","\""),
            // handle pseudo-spaces
            MosesPreprocessor.CreateRegexWithReplacement( @" \%", "%"),
            MosesPreprocessor.CreateRegexWithReplacement( @"nº ", "nº "),
            MosesPreprocessor.CreateRegexWithReplacement( @" :", ":"),
            MosesPreprocessor.CreateRegexWithReplacement( @" ºC", " ºC"),
            MosesPreprocessor.CreateRegexWithReplacement( @" cm", " cm"),
            MosesPreprocessor.CreateRegexWithReplacement( @" \?", "?"),
            MosesPreprocessor.CreateRegexWithReplacement( @" \!", "!"),
            MosesPreprocessor.CreateRegexWithReplacement( @" ;", ";"),
            MosesPreprocessor.CreateRegexWithReplacement( @", ", ", "),
            MosesPreprocessor.CreateRegexWithReplacement( @" +", " ")
        };

        private static List<Tuple<Regex, String>> PennRegexes = new List<Tuple<Regex, string>>()
        {
            MosesPreprocessor.CreateRegexWithReplacement(@"\`","'"),
            MosesPreprocessor.CreateRegexWithReplacement(@"\'\'"," \" ")
        };

        private static List<Tuple<Regex, String>> EngRegexes = new List<Tuple<Regex, string>>()
        {
            MosesPreprocessor.CreateRegexWithReplacement("\"([,\\.]+)", "$1\"")
        };

        private static List<Tuple<Regex, String>> GerSpaPunctRegexes = new List<Tuple<Regex, string>>()
        {
            MosesPreprocessor.CreateRegexWithReplacement(",\"","\","),
            MosesPreprocessor.CreateRegexWithReplacement(" (\\.+)\"(\\s*[^<])","\"$1$2") // don't fix period at end of sentence
        };

        private static List<Tuple<Regex, String>> CommaNumberRegex = new List<Tuple<Regex, string>>()
        {
            MosesPreprocessor.CreateRegexWithReplacement(@"(\d) (\d)","$1,$2")
        };

        private static List<Tuple<Regex, String>> DotNumberRegex = new List<Tuple<Regex, string>>()
        {
            MosesPreprocessor.CreateRegexWithReplacement(@"(\d) (\d)","$1.$2")
        };

        private static Tuple<Regex, String> CreateRegexWithReplacement(String pattern, String replacement)
        {
            return new Tuple<Regex, string>(new Regex(pattern, RegexOptions.Compiled), replacement);
        }

        private static String ApplyRegexReplacementCollection(String input, List<Tuple<Regex, String>> collection)
        {
            foreach (var regexReplacement in collection)
            {
                input = regexReplacement.Item1.Replace(input, regexReplacement.Item2);
            }
            return input;
        }

        public static string NormalizePunctuation(string input,string language)
        {
            var penn = 0;

            input = MosesPreprocessor.ApplyRegexReplacementCollection(input, MosesPreprocessor.PunctuationRegexes1);

            // normalize unicode punctuation
            if (penn == 0)
            {
                input = MosesPreprocessor.ApplyRegexReplacementCollection(input, MosesPreprocessor.PennRegexes);
            }

            input = MosesPreprocessor.ApplyRegexReplacementCollection(input, MosesPreprocessor.PunctuationRegexes2);

            // English "quotation," followed by comma, style
            if (language == "en") {
                input = MosesPreprocessor.ApplyRegexReplacementCollection(input, MosesPreprocessor.EngRegexes);
            }
            // Czech is confused
            else if(language == "cs" || language == "cz") {
            }

            // German/Spanish/French "quotation", followed by comma, style
            else {
                input = MosesPreprocessor.ApplyRegexReplacementCollection(input, MosesPreprocessor.GerSpaPunctRegexes);
            }
            
            if (language == "de" || language == "es" || language == "cz" || language == "cs" || language == "fr") {
                input = MosesPreprocessor.ApplyRegexReplacementCollection(input, MosesPreprocessor.CommaNumberRegex);
            }
            else {
                input = MosesPreprocessor.ApplyRegexReplacementCollection(input, MosesPreprocessor.DotNumberRegex);
            }

            return input;
        }
        
    }
}
