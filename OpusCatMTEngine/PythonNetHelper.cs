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
        private static Dictionary<IsoLanguage, dynamic> LemmatizerScopes = new Dictionary<IsoLanguage, dynamic>();

        internal static string Lemmatize(IsoLanguage lang, string input)
        {
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
                                $"stanza_wrapper.StanzaWrapper('{lang.ShortestIsoCode}', processors='tokenize, pos, lemma, depparse')");
                        }
                    }
                }

                return PythonNetHelper.LemmatizerScopes[lang].lemmatize(input);
            }
        }
    }
}
