
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    public interface IMtProvider
    {
        Task<TranslationPair> Translate(string inputString, IsoLanguage sourceLang, IsoLanguage targetLang, string modelTag);
        List<string> GetLanguagePairModelTags(string srcLangCode, string trgLangCode);
        List<string> GetAllLanguagePairs();
        string CheckModelStatus(IsoLanguage srcLangCode, IsoLanguage trgLangCode, string modelTag);
        bool FinetuningOngoing { get; }
        bool BatchTranslationOngoing { get; }
        void StartCustomization(
            List<Tuple<string, string>> input,
            List<Tuple<string, string>> validation,
            List<string> uniqueNewSegments,
            IsoLanguage srcLang,
            IsoLanguage trgLang,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs,
            MTModel baseModel);
    }
}
