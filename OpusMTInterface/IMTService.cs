using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace OpusMTInterface
{
    [ServiceContract]
    public interface IMTService
    {
        [OperationContract]
        [WebGet]
        string Login(string userName, string password);
        
        [OperationContract]
        [WebGet]
        List<string> GetLanguagePairModelTags(string tokenCode, string srcLangCode, string trgLangCode);

        [OperationContract]
        [WebGet]
        List<string> ListSupportedLanguagePairs(string tokenCode);

        [OperationContract]
        [WebGet]
        string Translate(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag);

        //Wordfast POSTs Custom MT requests, so it needs a POST method
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat=WebMessageFormat.Json, UriTemplate = "TranslatePost?tokenCode={tokenCode}&input={input}&srcLangCode={srcLangCode}&trgLangCode={trgLangCode}&modelTag={modelTag}")]
        Translation TranslatePost(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        Translation TranslateJson(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag);

        [OperationContract]
        [WebGet]
        Stream TranslateStream(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag);

        [OperationContract]
        [WebGet]
        string CheckModelStatus(string tokenCode, string srcLangCode, string trgLangCode, string modelTag);

        /*[OperationContract]
        [WebInvoke(Method = "POST",BodyStyle = WebMessageBodyStyle.Wrapped)]
        string Customize(
            string tokenCode,
            List<ParallelSentence> input,
            List<ParallelSentence> validation,
            List<string> uniqueNewSegments,
            string srcLangCode,
            string trgLangCode,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs);*/

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<string> BatchTranslate(string tokenCode, List<string> input, string srcLangCode, string trgLangCode, string modelTag);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string PreOrderBatch(string tokenCode, List<string> input, string srcLangCode, string trgLangCode, String modelId);
        
        [OperationContract]
        [WebGet]
        void StoreTranslation(string tokenCode, string source, string target, string srcLangCode, string trgLangCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int[] BatchStoreTranslation(string tokenCode, List<string> sources, List<string> targets, string srcLangCode, string trgLangCode);
        
    }
}
