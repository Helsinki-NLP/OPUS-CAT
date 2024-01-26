using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;

namespace OpusCatMtEngine
{
    static class PythonNetHelper
    {
        private static Dictionary<string, dynamic> LemmatizerScopes = new Dictionary<string, dynamic>();
        
        internal static List<Tuple<int,int,string>> Lemmatize(string lang, string input)
        {
            List<Tuple<int, int, string>> lemmaList = new List<Tuple<int, int, string>>();
            using (Py.GIL())
            {
                var output = new List<Tuple<int, int, string>>();
                if (!LemmatizerScopes.ContainsKey(lang))
                {
                    dynamic stanza = Py.Import("stanza");
                    LemmatizerScopes[lang] = stanza.Pipeline(lang, processors: "tokenize, pos, lemma, depparse");
                }

                dynamic processed = LemmatizerScopes[lang](input);
                foreach (var sentence in processed.sentences)
                {
                    foreach (var word in sentence.words)
                    {
                        output.Add(new Tuple<int, int, string>((int)word.start_char, (int)word.end_char, (string)word.lemma));
                    }
                }
                
                return output;
            }
            
        }
    }
}
