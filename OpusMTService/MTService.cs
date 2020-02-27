using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using OpusMTInterface;

namespace OpusMTService
{

    /// <summary>
    /// Dummy web service which can be called by the dummy MT plugin.
    /// </summary>
    /// <remarks>
    /// Implementation checklist:
    ///     - The MTException class is used to wrap the original exceptions occurred during the translation.
    ///     - All allocated resources are disposed correctly in the session.
    /// </remarks>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class MTService : IMTService
    {
        private ModelManager modelManager;
        private MarianManager marianManager;

        public MTService(ModelManager modelManager, MarianManager marianManager)
        {
            this.modelManager = modelManager;
            this.marianManager = marianManager;
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
            
            return modelManager.GetAllLanguagePairs().ToList();
        }

        /// <summary>
        /// Call this method to get the translation for a single string.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="input">The input string.</param>
        /// <param name="srcLangCode">The code of the source language.</param>
        /// <param name="trgLangCode">The code of the target language.</param>
        /// <returns>The translated input string.</returns>
        public string Translate(string tokenCode, string input, string srcLangCode, string trgLangCode)
        {
         
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            return this.modelManager.Translate(input, srcLangCode, trgLangCode);
;
        }

        /// <summary>
        /// Call this method to get the translation for multiple strings in batch.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="input">The input strings.</param>
        /// <param name="srcLangCode">The code of the source language.</param>
        /// <param name="trgLangCode">The code of the target language.</param>
        /// <returns>The translated input strings.</returns>
        public List<string> BatchTranslate(string tokenCode, List<string> input, string srcLangCode, string trgLangCode)
        {
            
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            List<string> result = new List<string>();
            foreach (string item in input)
            {
                result.Add(this.modelManager.Translate(item, srcLangCode, trgLangCode));
            }

            return result;
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
    }
}
