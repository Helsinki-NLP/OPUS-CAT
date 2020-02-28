using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using OpusMTInterface;

namespace OpusMTPlugin
{
    /// <summary>
    /// Helper class to be able to communicate with the web service.
    /// </summary>
    /// <remarks>
    /// Implementation checklist:
    ///     - The MTException class is used to wrap the original exceptions occurred during the translation.
    ///     - All allocated resources are disposed correctly in the session.
    /// </remarks>
    internal class OpusMTServiceHelper
    {
        private static DateTime TokenCodeExpires = DateTime.MinValue;
        private static string TokenCode;

        public static IMTService getNewProxy()
        {
            var epAddr = new EndpointAddress("  net.tcp://localhost:8733/MTService");
            return ChannelFactory<IMTService>.CreateChannel(new NetTcpBinding(), epAddr);
        }

        /// <summary>
        /// Gets the valid token code.
        /// </summary>
        /// <returns>The token code.</returns>
        public static string GetTokenCode(OpusMTOptions options)
        {
            if (TokenCodeExpires < DateTime.Now)
            {
                // refresh the token code
                // Always dispose allocated resources
                var proxy = getNewProxy();
                using (proxy as IDisposable)
                {
                    TokenCode = proxy.Login(options.SecureSettings.UserName, options.SecureSettings.Password);
                    TokenCodeExpires = DateTime.Now.AddMinutes(1);
                }
            }

            return TokenCode;
        }

        /// <summary>
        /// Calls the web service's login method.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The token code.</returns>
        public static string Login(string userName, string password)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy();
            using (proxy as IDisposable)
            {
                return proxy.Login(userName, password);
            }
        }

        /// <summary>
        /// Lists the supported languages of the dummy MT service.
        /// </summary>
        /// <returns>The list of the supported languages.</returns>
        public static List<string> ListSupportedLanguages(OpusMTOptions options)
        {
            return ListSupportedLanguages(GetTokenCode(options));
        }

        /// <summary>
        /// Lists the supported languages of the dummy MT service.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <returns>The list of the supported languages.</returns>
        public static List<string> ListSupportedLanguages(string tokenCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy();
            using (proxy as IDisposable)
            {
                string[] supportedLanguages = proxy.ListSupportedLanguagePairs(tokenCode).ToArray();
                return supportedLanguages.ToList();
            }
        }

        /// <summary>
        /// Translates a single string with the help of the dummy MT service.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="input">The string to translate.</param>
        /// <param name="srcLangCode">The source language code.</param>
        /// <param name="trgLangCode">The target language code.</param>
        /// <returns>The translated string.</returns>
        public static string Translate(OpusMTOptions options, string input, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy();
            using (proxy as IDisposable)
            {
                string result = proxy.Translate(GetTokenCode(options), input, srcLangCode, trgLangCode);
                return result;
            }
        }

        /// <summary>
        /// Translates multiple strings with the help of the dummy MT service.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="input">The strings to translate.</param>
        /// <param name="srcLangCode">The source language code.</param>
        /// <param name="trgLangCode">The target language code.</param>
        /// <returns>The translated strings.</returns>
        public static List<string> BatchTranslate(OpusMTOptions options, List<string> input, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy();
            using (proxy as IDisposable)
            {
                string[] result = proxy.BatchTranslate(GetTokenCode(options), input, srcLangCode, trgLangCode).ToArray();
                return result.ToList();
            }
        }

        /// <summary>
        /// Stores a single string pair as translation with the help of the dummy MT service.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="srcLangCode">The source language code.</param>
        /// <param name="trgLangCode">The target language code.</param>
        public static void StoreTranslation(OpusMTOptions options, string source, string target, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy();
            using (proxy as IDisposable)
            {
                proxy.StoreTranslation(GetTokenCode(options), source, target, srcLangCode, trgLangCode);
            }
        }

        /// <summary>
        /// Stores multiple string pairs as translation with the help of the dummy MT service.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="sources">The source strings.</param>
        /// <param name="targets">The target strings.</param>
        /// <param name="srcLangCode">The source language code.</param>
        /// <param name="trgLangCode">The target language code.</param>
        /// <returns>The indices of the translation units that were succesfully stored.</returns>
        public static int[] BatchStoreTranslation(OpusMTOptions options, List<string> sources, List<string> targets, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy();
            using (proxy as IDisposable)
            {
                return proxy.BatchStoreTranslation(GetTokenCode(options), sources, targets, srcLangCode, trgLangCode);
            }
        }
    }
}
