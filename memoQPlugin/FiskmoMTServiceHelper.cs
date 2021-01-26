using OpusMTInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace FiskmoMTPlugin
{
    /// <summary>
    /// Helper class to be able to communicate with the web service.
    /// </summary>
    /// <remarks>
    /// Implementation checklist:
    ///     - The MTException class is used to wrap the original exceptions occurred during the translation.
    ///     - All allocated resources are disposed correctly in the session.
    /// </remarks>
    internal class FiskmoMTServiceHelper
    {
        private static DateTime TokenCodeExpires = DateTime.MinValue;
        private static string TokenCode;

        public static IMTService getNewProxy(string port)
        {
            
            var epAddr = new EndpointAddress($"net.tcp://localhost:{port}/MTService");
            return ChannelFactory<IMTService>.CreateChannel(new NetTcpBinding(), epAddr);
        }

        /// <summary>
        /// Gets the valid token code.
        /// </summary>
        /// <returns>The token code.</returns>
        public static string GetTokenCode(FiskmoMTOptions options)
        {
            if (TokenCodeExpires < DateTime.Now)
            {
                // refresh the token code
                // Always dispose allocated resources
                var proxy = getNewProxy(options.GeneralSettings.MtServicePort);
                using (proxy as IDisposable)
                {
                    TokenCode = proxy.Login("user", "user");
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
        public static string Login(string userName, string password, string port)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(port);
            using (proxy as IDisposable)
            {
                return proxy.Login(userName, password);
            }
        }

        /// <summary>
        /// Lists the supported languages of the dummy MT service.
        /// </summary>
        /// <returns>The list of the supported languages.</returns>
        public static List<string> ListSupportedLanguages(FiskmoMTOptions options)
        {
            return ListSupportedLanguages(GetTokenCode(options),options.GeneralSettings.MtServicePort);
        }

        /// <summary>
        /// Lists the supported languages of the dummy MT service.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <returns>The list of the supported languages.</returns>
        public static List<string> ListSupportedLanguages(string tokenCode, string port)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(port);
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
        public static string Translate(FiskmoMTOptions options, string input, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(options.GeneralSettings.MtServicePort);
            using (proxy as IDisposable)
            {
                string result = proxy.Translate(GetTokenCode(options), input, srcLangCode, trgLangCode,"");
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
        public static List<string> BatchTranslate(FiskmoMTOptions options, List<string> input, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(options.GeneralSettings.MtServicePort);
            using (proxy as IDisposable)
            {
                string[] result = proxy.BatchTranslate(GetTokenCode(options), input, srcLangCode, trgLangCode,"").ToArray();
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
        public static void StoreTranslation(FiskmoMTOptions options, string source, string target, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(options.GeneralSettings.MtServicePort);
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
        public static int[] BatchStoreTranslation(FiskmoMTOptions options, List<string> sources, List<string> targets, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(options.GeneralSettings.MtServicePort);
            using (proxy as IDisposable)
            {
                return proxy.BatchStoreTranslation(GetTokenCode(options), sources, targets, srcLangCode, trgLangCode);
            }
        }
    }
}
