using OpusMTInterface;
using Sdl.LanguagePlatform.TranslationMemory.EditScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace FiskmoTranslationProvider
{

    internal class FiskmöMTServiceHelper
    {
        private static Random rng = new Random();
        private static DateTime TokenCodeExpires = DateTime.MinValue;
        private static string TokenCode;

        public static IMTService getNewProxy(string host, string port)
        {
            NetTcpBinding myBinding = new NetTcpBinding();

            //Use default net.tcp security, which is based on Windows authentication:
            //can only contact services in the same domain.
            //TODO: add a checkbox (with warning) in the UI for using security mode None,
            //to allow connections from IP range (also add same checkbox to service UI).

            //myBinding.Security.Mode = SecurityMode.None;
            //myBinding.Security.Mode = SecurityMode.Transport;
            //myBinding.Security.Transport.ClientCredentialType =
            //    TcpClientCredentialType.Windows;

            var epAddr = new EndpointAddress($"net.tcp://{host}:{port}/MTService");
            var proxy = ChannelFactory<IMTService>.CreateChannel(myBinding, epAddr);

            return proxy;
        }

        /// <summary>
        /// Gets the valid token code.
        /// </summary>
        /// <returns>The token code.</returns>
        /// 

        public static string GetTokenCode(string host, string mtServicePort)
        {
            if (TokenCodeExpires < DateTime.Now)
            {
                // refresh the token code
                // Always dispose allocated resources
                var proxy = getNewProxy(host, mtServicePort);
                try
                {
                    using (proxy as IDisposable)
                    {
                        TokenCode = proxy.Login("user", "user");
                        TokenCodeExpires = DateTime.Now.AddMinutes(1);
                    }
                }
                catch (Exception ex) when (ex.InnerException is SocketException)
                {
                    throw new OpusCatEngineConnectionException("OPUS-CAT MT Engine cannot be connected to. Check that the OPUS-CAT MT Engine is running.", ex);
                }
                catch (Exception ex) when (ex is EndpointNotFoundException || ex is CommunicationObjectFaultedException)
                {
                    throw new OpusCatEngineConnectionException("OPUS-CAT MT Engine cannot be connected to. Check that the OPUS-CAT MT Engine is running.", ex);
                }
                catch (Exception ex)
                {
                    //If the server throws an previously unseen exception, this will probably catch it.
                    //This is here mainly for future debugging, no exceptions of this type are currently thrown.
                    throw ex;
                }
            }

            return TokenCode;
        }


        public static List<string> GetLanguagePairModelTags(FiskmoOptions options, string srcLangCode, string trgLangCode)
        {
            return FiskmöMTServiceHelper.GetLanguagePairModelTags(options.mtServiceAddress, options.mtServicePort, srcLangCode, trgLangCode);
        }

        public static List<string> GetLanguagePairModelTags(string host, string port, string srcLangCode, string trgLangCode)
        {
            var proxy = getNewProxy(host, port);
            using (proxy as IDisposable)
            {
                List<string> modelTags = proxy.GetLanguagePairModelTags(GetTokenCode(host, port), srcLangCode, trgLangCode);
                return modelTags;
            }
        }

        public static string CheckModelStatus(string host, string port, string srcLangCode, string trgLangCode, string modelTag)
        {
            var proxy = getNewProxy(host, port);
            using (proxy as IDisposable)
            {
                string status = proxy.CheckModelStatus(GetTokenCode(host, port), srcLangCode, trgLangCode, modelTag);
                return status;
            }
        }

        public static string GetTokenCode(FiskmoOptions options)
        {
            return FiskmöMTServiceHelper.GetTokenCode(options.mtServiceAddress, options.mtServicePort);
        }

        /// <summary>
        /// Calls the web service's login method.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The token code.</returns>
        public static string Login(string host, string userName, string password, string port)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(host, port);
            using (proxy as IDisposable)
            {
                return proxy.Login(userName, password);
            }
        }


        public static List<string> ListSupportedLanguages(FiskmoOptions options)
        {
            return ListSupportedLanguages(GetTokenCode(options), options.mtServiceAddress, options.mtServicePort);
        }

        public static List<string> ListSupportedLanguages(string host, string port)
        {
            return ListSupportedLanguages(GetTokenCode(host, port), host, port);
        }

        /// <summary>
        /// Lists the supported languages of the dummy MT service.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <returns>The list of the supported languages.</returns>
        public static List<string> ListSupportedLanguages(string tokenCode, string host, string port)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(host, port);
            using (proxy as IDisposable)
            {
                string[] supportedLanguages = proxy.ListSupportedLanguagePairs(tokenCode).ToArray();
                return supportedLanguages.ToList();
            }
        }


        public static string Translate(FiskmoOptions options, string input, string srcLangCode, string trgLangCode, string modelTag)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(options.mtServiceAddress, options.mtServicePort);
            using (proxy as IDisposable)
            {
                string result = proxy.Translate(GetTokenCode(options), input, srcLangCode, trgLangCode, modelTag);
                return result;
            }
        }

        public static void PreOrder(FiskmoOptions options, string input, string srcLangCode, string trgLangCode, string modelTag)
        {
            Task.Run(() =>
            {
                // Always dispose allocated resources
                var proxy = getNewProxy(options.mtServiceAddress, options.mtServicePort);
                using (proxy as IDisposable)
                {
                    proxy.Translate(GetTokenCode(options), input, srcLangCode, trgLangCode, modelTag);
                }
            });
        }

        internal static string PreTranslateBatch(string host, string mtServicePort, List<string> projectNewSegments, string sourceCode, string targetCode, string modelTag)
        {
            var proxy = getNewProxy(host, mtServicePort);

            using (proxy as IDisposable)
            {
                return proxy.PreTranslateBatch(GetTokenCode(host, mtServicePort), projectNewSegments, sourceCode, targetCode, modelTag);
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
        public static List<string> BatchTranslate(FiskmoOptions options, List<string> input, string srcLangCode, string trgLangCode, string modelTag)
        {
            // Always dispose allocated resources
            var proxy = getNewProxy(options.mtServiceAddress,options.mtServicePort);
            using (proxy as IDisposable)
            {
                string[] result = proxy.BatchTranslate(GetTokenCode(options), input, srcLangCode, trgLangCode,modelTag).ToArray();
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
            var proxy = getNewProxy(options.mtServiceAddress, options.mtServicePort);
            using (proxy as IDisposable)
            {
                proxy.StoreTranslation(GetTokenCode(options), source, target, srcLangCode, trgLangCode);
            }
        }

        internal static string Customize(
            string host,
            string mtServicePort,
            List<Tuple<string, string>> projectTranslations,
            List<string> uniqueNewSegments,
            string sourceCode,
            string targetCode,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            var proxy = getNewProxy(host, mtServicePort);

            //Pick out 200 sentence pairs randomly to use as tuning set
            var randomTranslations = projectTranslations.OrderBy(x => rng.Next());
            var trainingSet = randomTranslations.Skip(200).ToList();
            var validSet = randomTranslations.Take(200).ToList();
            string result;
            using (proxy as IDisposable)
            {
                result = proxy.Customize(GetTokenCode(host,mtServicePort), trainingSet, validSet, uniqueNewSegments, sourceCode, targetCode, modelTag, includePlaceholderTags, includeTagPairs);
            }
            return result;
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
            var proxy = getNewProxy(options.mtServiceAddress, options.mtServicePort);
            using (proxy as IDisposable)
            {
                return proxy.BatchStoreTranslation(GetTokenCode(options), sources, targets, srcLangCode, trgLangCode);
            }
        }
    }
}
