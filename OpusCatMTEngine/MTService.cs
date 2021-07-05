using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpusMTInterface;

namespace OpusCatMTEngine
{


    [ServiceBehavior(
        InstanceContextMode=InstanceContextMode.Single,
        AddressFilterMode = AddressFilterMode.Any,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class MTService : IMTService
    {
        
        public ModelManager ModelManager { get; internal set; }

        public MTService(ModelManager modelManager)
        {
            this.ModelManager = modelManager;
        }

        /// <summary>
        /// Call this method to get a token code for the further
        /// calls.Returns the token code is the credentials are
        /// valid (this dummy service allows the login request
        /// if the user name and the password are identical), 
        /// otherwise returns null.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The token code if the credentials are valid, null otherwise.</returns>
        public string Login(string userName, string password)
        {
            return userName.Equals(password) ? TokenCodeGenerator.Instance.GenerateTokenCode(userName) : null;
        }

        public List<string> GetLanguagePairModelTags(string tokenCode, string srcLangCode, string trgLangCode)
        {
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            return this.ModelManager.GetLanguagePairModelTags(srcLangCode,trgLangCode);
        }

        /// <summary>
        /// Call this method to get the supported languages of the service.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <returns>The supported languages.</returns>
        public List<string> ListSupportedLanguagePairs(string tokenCode)
        {
         
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;
            
            return this.ModelManager.GetAllLanguagePairs();
        }

        public string CheckModelStatus(string tokenCode, string sourceCode, string targetCode, string modelTag)
        {
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            var sourceLang = new IsoLanguage(sourceCode);
            var targetLang = new IsoLanguage(targetCode);
            
            return this.ModelManager.CheckModelStatus(sourceLang, targetLang, modelTag);
        }

        /// <summary>
        /// Call this method to get the translation for a single string.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="input">The input string.</param>
        /// <param name="srcLangCode">The code of the source language.</param>
        /// <param name="trgLangCode">The code of the target language.</param>
        /// <returns>The translated input string.</returns>
        public string Translate(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag)
        {    
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            return this.ModelManager.Translate(input, sourceLang, targetLang, modelTag).Result.Translation;
        }

        //For integration with Wordfast
        public Translation TranslatePost(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag)
        {
            var translation = this.Translate(tokenCode, input, srcLangCode, trgLangCode, modelTag);
            return new Translation(translation);
        }

        public Translation TranslateJson(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin: *");

            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            var translation = this.ModelManager.Translate(input, sourceLang, targetLang, modelTag);
            return new Translation(translation.Result.Translation);
        }


        public Stream TranslateStream(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag)
        {
            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain; charset=utf-8";
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Connection: close");
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin: *");

            //This is for Wordfast Anywhere (probably other versions as well) compatibility, for some reason it doesn't accept a response with
            //the default Server header.
            WebOperationContext.Current.OutgoingResponse.Headers.Add(HttpResponseHeader.Server.ToString(), string.Empty);
            
            var translation = this.ModelManager.Translate(input, sourceLang, targetLang, modelTag).Result;
            return new MemoryStream(Encoding.UTF8.GetBytes(translation.Translation));
        }

        

        /// <summary>
        /// Call this method to get the translation for multiple strings in batch.
        /// NOTE: this is not currently used, for pretranslation PretranslateBatch is better.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="input">The input strings.</param>
        /// <param name="srcLangCode">The code of the source language.</param>
        /// <param name="trgLangCode">The code of the target language.</param>
        /// <returns>The translated input strings.</returns>
        public List<string> BatchTranslate(string tokenCode, List<string> input, string srcLangCode, string trgLangCode, string modelTag)
        {
            
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            List<TranslationPair> translations = new List<TranslationPair>();
            foreach (var sourceSegment in input)
            {
                translations.Add(this.ModelManager.Translate(sourceSegment, sourceLang, targetLang, modelTag).Result);
            }
            
            return translations.Select(x => x.Translation).ToList();
        }

        /// <summary>
        /// This will send a batch to the MT engine for pretranslation, which means
        /// the translations for the batch will be immediately available when requested
        /// </summary>
        /// <param name="tokenCode"></param>
        /// <param name="input"></param>
        /// <param name="srcLangCode"></param>
        /// <param name="trgLangCode"></param>
        public string PreOrderBatch(string tokenCode, List<string> input, string srcLangCode, string trgLangCode, string modelTag)
        {

            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return "";

            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            if (input.Count == 0)
            {
                return "input was empty";
            }

            foreach (var inputString in input)
            {
                this.ModelManager.Translate(inputString, sourceLang, targetLang, modelTag);
            }

            /* Batch preordering was done earlier with batch translation, but it doesn't seem
             * to be much quicker than normal translation, and it has the problem of providing all
             * the translations at once in the end. Using normal translation means the MT is ready
             * as soon as a sentence gets translated (you could do this for batch translation as well
             * by adding an outputline handler, but it's not implemented yet). Batch translation should be
             * much quicker, need to test for correct parameters, so stick with this. Using normal translate
             * is also more robust, one less thing to break.
            if (!this.ModelManager.BatchTranslationOngoing && !this.ModelManager.CustomizationOngoing)
            {
                this.ModelManager.PreTranslateBatch(input, sourceLang, targetLang, modelTag);
                return "batch translation started";
            }
            else
            {
                return "batch translation or customization already in process";
            }*/

            return "preorder received";
        }


        public void StoreTranslation(string tokenCode, string source, string target, string srcLangCode, string trgLangCode)
        {
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return;
        }

        public int[] BatchStoreTranslation(string tokenCode, List<string> sources, List<string> targets, string srcLangCode, string trgLangCode)
        {
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return new int[0];

            var indicesAdded = new int[sources.Count];
            for (int i = 0; i < sources.Count; ++i)
            {
                
            }
            return indicesAdded;
        }

        public string Customize(
            string tokenCode,
            List<ParallelSentence> input,
            List<ParallelSentence> validation,
            List<string> uniqueNewSegments,
            string srcLangCode,
            string trgLangCode,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            if (!this.ModelManager.FinetuningOngoing && !this.ModelManager.BatchTranslationOngoing)
            {
                this.ModelManager.StartCustomization(
                    input, validation, uniqueNewSegments, sourceLang, targetLang, modelTag, includePlaceholderTags, includeTagPairs);
                return "fine-tuning started";
            }
            else
            {
                //TODO: need to queue up customization, i.e. save data for starting later
                throw new FaultException($"Batch translation or customization already in process in the MT engine");
            }
        }
        
    }
}
