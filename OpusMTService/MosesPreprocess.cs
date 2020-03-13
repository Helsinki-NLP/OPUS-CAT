using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusMTService
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
            input = ReplaceUnicodePunctuation(input);
            input = RemoveNonPrintingChar(input);
            input = NormalizePunctuation(input,language);
            return input;
        }

        public static string ReplaceUnicodePunctuation(string input)
        {
            input = Regex.Replace(input, "，", ",");
            input = Regex.Replace(input, "。 *", ". ");
            input = Regex.Replace(input, "、", ",");
            input = Regex.Replace(input, "”", "\"");
            input = Regex.Replace(input, "“", "\"");
            input = Regex.Replace(input, "∶", ":");
            input = Regex.Replace(input, "：", ":");
            input = Regex.Replace(input, "？", "?");
            input = Regex.Replace(input, "《", "\"");
            input = Regex.Replace(input, "》", "\"");
            input = Regex.Replace(input, "）", ")");
            input = Regex.Replace(input, "！", "!");
            input = Regex.Replace(input, "（", "(");
            input = Regex.Replace(input, "；", ";");
            input = Regex.Replace(input, "１", "\"");
            input = Regex.Replace(input, "」", "\"");
            input = Regex.Replace(input, "「", "\"");
            input = Regex.Replace(input, "０", "0");
            input = Regex.Replace(input, "３", "3");
            input = Regex.Replace(input, "２", "2");
            input = Regex.Replace(input, "５", "5");
            input = Regex.Replace(input, "６", "6");
            input = Regex.Replace(input, "９", "9");
            input = Regex.Replace(input, "７", "7");
            input = Regex.Replace(input, "８", "8");
            input = Regex.Replace(input, "４", "4");
            input = Regex.Replace(input, "． *", ". ");
            input = Regex.Replace(input, "～", "~");
            input = Regex.Replace(input, "’", "'");
            input = Regex.Replace(input, "…", "...");
            input = Regex.Replace(input, "━", "-");
            input = Regex.Replace(input, "〈", "<");
            input = Regex.Replace(input, "〉", ">");
            input = Regex.Replace(input, "【", "[");
            input = Regex.Replace(input, "】", "]");
            input = Regex.Replace(input, "％", "%");

            return input;
        }

        public static string RemoveNonPrintingChar(string input)
        {
            input = Regex.Replace(input,@"\p{C}"," ");
            return input;
        }

        public static string NormalizePunctuation(string input,string language)
        {
            var penn = 0;
            input = Regex.Replace(input, "\r", "");
            // remove extra spaces
            input = Regex.Replace(input, @"\(", " (");
            input = Regex.Replace(input, @"\)", ") ");
            input = Regex.Replace(input, @" +", " ");
            input = Regex.Replace(input, @"\) ([\.\!\:\?\;\,])", ")$1");
            input = Regex.Replace(input, @"\( ", "(");
            input = Regex.Replace(input, @" \)", ")");
            input = Regex.Replace(input, @"(\d) \%", "$1%");
            input = Regex.Replace(input, @" :", ":");
            input = Regex.Replace(input, @" ;", ";");
            // normalize unicode punctuation
            if (penn == 0)
            {
                input = Regex.Replace(input, @"\`", "'");
                input = Regex.Replace(input, @"\'\'", " \" ");
            }

            input = Regex.Replace(input, @"„", "\"");
            input = Regex.Replace(input,@"“","\"");
            input = Regex.Replace(input, @"”", "\"");
            input = Regex.Replace(input,@"–"," - ");
        
            input = Regex.Replace(input, @"—", " - ");
            input = Regex.Replace(input, @" +", " ");
            input = Regex.Replace(input, @"´", "'");
            input = Regex.Replace(input, @"([a-z])‘([a-z])", @"$1\'$2",RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"([a-z])’([a-z])", @"$1\'$2", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"‘", "\"");
            input = Regex.Replace(input,@"‚","\"");
            input = Regex.Replace(input, @"’", "\"");
            input = Regex.Replace(input,@"''","\"");
            input = Regex.Replace(input, @"´´", "\"");
            input = Regex.Replace(input,@"…",@"...");
            // French quotes
            input = Regex.Replace(input, @" « ", " \"");
            input = Regex.Replace(input,@"« ","\"");
            input = Regex.Replace(input, @"«", "\"");
            input = Regex.Replace(input,@" » ","\" ");
            input = Regex.Replace(input, @" »", "\"");
            input = Regex.Replace(input,@"»","\"");
            // handle pseudo-spaces
            input = Regex.Replace(input, @" \%", "%");
            input = Regex.Replace(input, @"nº ", "nº ");
            input = Regex.Replace(input, @" :", ":");
            input = Regex.Replace(input, @" ºC", " ºC");
            input = Regex.Replace(input, @" cm", " cm");
            input = Regex.Replace(input, @" \?", "?");
            input = Regex.Replace(input, @" \!", "!");
            input = Regex.Replace(input, @" ;", ";");
            input = Regex.Replace(input, @", ", ", ");
            input = Regex.Replace(input, @" +", " ");

            // English "quotation," followed by comma, style
            if (language == "en") {
                input = Regex.Replace(input, "\"([,\\.]+)", "$1\"");
            }
            // Czech is confused
            else if(language == "cs" || language == "cz") {
            }

            // German/Spanish/French "quotation", followed by comma, style
            else {
                input = Regex.Replace(input, ",\"","\",");
                input = Regex.Replace(input, " (\\.+)\"(\\s*[^<])","\"$1$2"); // don't fix period at end of sentence
            }
            
            if (language == "de" || language == "es" || language == "cz" || language == "cs" || language == "fr") {
                input = Regex.Replace(input,@"(\d) (\d)","$1,$2");
            }
            else {
                input = Regex.Replace(input,@" (\d) (\d)","$1.$2");
            }

            return input;
        }
        
    }
}
