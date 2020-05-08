using OpusMTInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;

namespace FiskmoTranslationProvider
{
    /// <summary>
    /// Helper class to be able to communicate with the web service.
    /// </summary>
    /// <remarks>
    /// Implementation checklist:
    ///     - The MTException class is used to wrap the original exceptions occurred during the translation.
    ///     - All allocated resources are disposed correctly in the session.
    /// </remarks>
    internal class FiskmöMTServiceHelper
    {
        private static Random rng = new Random();
        private static DateTime TokenCodeExpires = DateTime.MinValue;
        private static string TokenCode;

        public static IMTService getNewProxy(string port)
        {
            
            var epAddr = new EndpointAddress($"net.tcp://localhost:{port}/MTService");
            var proxy = ChannelFactory<IMTService>.CreateChannel(new NetTcpBinding(), epAddr);
            return proxy;
        }

        /// <summary>
        /// Gets the valid token code.
        /// </summary>
        /// <returns>The token code.</returns>
        /// 

        public static string GetTokenCode(string mtServicePort)
        {
            if (TokenCodeExpires < DateTime.Now)
            {
                // refresh the token code
                // Always dispose allocated resources
                var proxy = getNewProxy(mtServicePort);
                try
                {
                    using (proxy as IDisposable)
                    {
                        TokenCode = proxy.Login("user", "user");
                        TokenCodeExpires = DateTime.Now.AddMinutes(1);
                    }
                }
                catch (Exception ex) when (ex is EndpointNotFoundException || ex is CommunicationObjectFaultedException)
                {
                    MessageBox.Show(
                        "No connection to Fiskmö MT service. Check that the MT service is running and that both plugin and MT service use same port numbers.");
                    throw ex;
                }
            }

            return TokenCode;
        }

        public static string GetTokenCode(FiskmoOptions options)
        {
            return FiskmöMTServiceHelper.GetTokenCode(options.mtServicePort);
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
        public static List<string> ListSupportedLanguages(FiskmoOptions options)
        {
            return ListSupportedLanguages(GetTokenCode(options),options.mtServicePort);
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
        public static string Translate(FiskmoOptions options, string input, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(options.mtServicePort);
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
        public static List<string> BatchTranslate(FiskmoOptions options, List<string> input, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(options.mtServicePort);
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
        public static void StoreTranslation(FiskmoOptions options, string source, string target, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(options.mtServicePort);
            using (proxy as IDisposable)
            {
                proxy.StoreTranslation(GetTokenCode(options), source, target, srcLangCode, trgLangCode);
            }
        }

        internal static void Customize(string mtServicePort, List<Tuple<string, string>> projectTranslations, string sourceCode, string targetCode)
        {
            var proxy = getNewProxy(mtServicePort);

            //Pick out 200 sentence pairs randomly to use as tuning set
            var randomTranslations = projectTranslations.OrderBy(x => rng.Next());
            var trainingSet = projectTranslations.Skip(200).ToList();
            var tuningSet = projectTranslations.Take(200).ToList();

            using (proxy as IDisposable)
            {
                proxy.Customize(GetTokenCode(mtServicePort), trainingSet, tuningSet, sourceCode, targetCode);
            }
        }
        
        internal static void Customize(FiskmoOptions options, List<Tuple<string, string>> tuningSet, string sourceCode, string targetCode)
        {
            FiskmöMTServiceHelper.Customize(options.mtServicePort, tuningSet, sourceCode, targetCode);
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
        public static int[] BatchStoreTranslation(FiskmoOptions options, List<string> sources, List<string> targets, string srcLangCode, string trgLangCode)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(options.mtServicePort);
            using (proxy as IDisposable)
            {
                return proxy.BatchStoreTranslation(GetTokenCode(options), sources, targets, srcLangCode, trgLangCode);
            }
        }
    }
}
