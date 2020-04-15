using System;
using System.Collections.Generic;
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
        List<string> ListSupportedLanguagePairs(string tokenCode);

        [OperationContract]
        [WebGet]
        string Translate(string tokenCode, string input, string srcLangCode, string trgLangCode);
        
        [OperationContract]
        [WebGet]
        string TranslateWithModel(string tokenCode, string input, string modelName);

        [OperationContract]
        [WebInvoke(Method = "POST",BodyStyle = WebMessageBodyStyle.Wrapped)]
        string Customize(string tokenCode, List<Tuple<string,string>> input, string srcLangCode, string trgLangCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<string> BatchTranslate(string tokenCode, List<string> input, string srcLangCode, string trgLangCode);

        [OperationContract]
        [WebGet]
        void StoreTranslation(string tokenCode, string source, string target, string srcLangCode, string trgLangCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int[] BatchStoreTranslation(string tokenCode, List<string> sources, List<string> targets, string srcLangCode, string trgLangCode);
    }
}
