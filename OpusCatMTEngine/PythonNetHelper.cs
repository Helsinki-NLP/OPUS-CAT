using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;

namespace OpusCatMTEngine
{
    static class PythonNetHelper
    {
        private static Dictionary<string, dynamic> LemmatizerScopes = new Dictionary<string, dynamic>();

        internal static List<Tuple<int,int,string>> Lemmatize(string lang, string input)
        {
            List<Tuple<int, int, string>> lemmaList = new List<Tuple<int, int, string>>();
            using (Py.GIL())
            {
                if (!PythonNetHelper.LemmatizerScopes.ContainsKey(lang))
                {
                    //Initialize the lemmatizer
                    using (var moduleScope = Py.CreateScope())
                    {
                        moduleScope.Exec(OpusCatMTEngine.Properties.Resources.StanzaWrapperCode);
                        // create a Python scope
                        using (PyScope scope = Py.CreateScope())
                        {
                            scope.Import(moduleScope, "stanza_wrapper");

                            PythonNetHelper.LemmatizerScopes[lang] = scope.Eval<dynamic>(
                                $"stanza_wrapper.StanzaWrapper('{lang}', processors='tokenize, pos, lemma, depparse')");
                        }
                    }
                }

                var lemmatized = PythonNetHelper.LemmatizerScopes[lang].lemmatize(input);
                var output = new List<Tuple<int, int, string>>();
                foreach (var lemma in lemmatized)
                {
                    output.Add(new Tuple<int,int,string>((int)lemma[0],(int)lemma[1],(string)lemma[2]));
                }
                return output;
            }
        }
    }
}
