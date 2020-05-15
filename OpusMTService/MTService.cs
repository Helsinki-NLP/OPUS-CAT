using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using OpusMTInterface;

namespace FiskmoMTEngine
{


    [ServiceBehavior(
        InstanceContextMode=InstanceContextMode.Single,
        AddressFilterMode = AddressFilterMode.Any)]
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

        /// <summary>
        /// Call this method to get the supported languages of the service.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <returns>The supported languages.</returns>
        public List<string> ListSupportedLanguagePairs(string tokenCode)
        {
         
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;
            
            return this.ModelManager.GetAllLanguagePairs().ToList();
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

            return this.ModelManager.Translate(input, srcLangCode, trgLangCode, modelTag);
        }
        
        /// <summary>
        /// Call this method to get the translation for a single string with the named model.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="input">The input string.</param>
        /// <param name="modelName">Name of the model to use.</param>
        /// <returns>The translated input string.</returns>
        public string TranslateWithModel(string tokenCode, string input, string modelName)
        {

            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            return this.ModelManager.TranslateWithModel(input, modelName);
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

            List<string> translations = new List<string>();
            foreach (var sourceSegment in input)
            {
                translations.Add(this.ModelManager.Translate(sourceSegment, srcLangCode, trgLangCode, modelTag));
            }
            
            return translations;
        }

        /// <summary>
        /// This will send a batch to the MT engine for pretranslation, which means
        /// the translations will be immediately available when it is requested
        /// </summary>
        /// <param name="tokenCode"></param>
        /// <param name="input"></param>
        /// <param name="srcLangCode"></param>
        /// <param name="trgLangCode"></param>
        public void PreTranslateBatch(string tokenCode, List<string> input, string srcLangCode, string trgLangCode, string modelTag)
        {

            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return;

            this.ModelManager.PreTranslateBatch(input, srcLangCode, trgLangCode, modelTag);

            return;
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

        public string Customize(string tokenCode, List<Tuple<string, string>> input, List<Tuple<string, string>> validation, List<string> uniqueNewSegments,string srcLangCode, string trgLangCode, string modelTag)
        {
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            this.ModelManager.Customize(input, validation, uniqueNewSegments, srcLangCode, trgLangCode, modelTag);

            return "tuning set received";
        }
    }
}
