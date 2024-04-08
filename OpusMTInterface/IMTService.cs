using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace OpusMTInterface
{
    [ServiceContract]
    public interface IMTService
    {
        [OperationContract]
        string Login(string userName, string password);
        
        [OperationContract]
        List<string> GetLanguagePairModelTags(string tokenCode, string srcLangCode, string trgLangCode);

        [OperationContract]
        List<string> ListSupportedLanguagePairs(string tokenCode);

        [OperationContract]
        string Translate(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag);

        //Wordfast POSTs Custom MT requests, so it needs a POST method
        [OperationContract]
        Translation TranslatePost(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag);

        [OperationContract]
        Translation TranslateJson(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag);

        [OperationContract]
        Stream TranslateStream(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag);

        [OperationContract]
        string CheckModelStatus(string tokenCode, string srcLangCode, string trgLangCode, string modelTag);

        [OperationContract]
        string Customize(
            string tokenCode,
            List<Tuple<string,string>> input,
            List<Tuple<string, string>> validation,
            List<string> uniqueNewSegments,
            string srcLangCode,
            string trgLangCode,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs);

        [OperationContract]
        List<string> BatchTranslate(string tokenCode, List<string> input, string srcLangCode, string trgLangCode, string modelTag);

        [OperationContract]
        string PreOrderBatch(string tokenCode, List<string> input, string srcLangCode, string trgLangCode, String modelId);
        
        [OperationContract]
        void StoreTranslation(string tokenCode, string source, string target, string srcLangCode, string trgLangCode);

        [OperationContract]
        int[] BatchStoreTranslation(string tokenCode, List<string> sources, List<string> targets, string srcLangCode, string trgLangCode);
        
    }
}
