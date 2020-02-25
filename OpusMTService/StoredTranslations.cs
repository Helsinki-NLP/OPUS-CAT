using System.Collections.Concurrent;

namespace OpusMTService
{
    /// <summary>
    /// Dummy implementation for storing translations in-memory.
    /// </summary>
    internal static class StoredTranslations
    {
        private static readonly ConcurrentDictionary<string, string> translations = new ConcurrentDictionary<string, string>();

        internal static void Store(string source, string target, string srcLangCode, string trgLangCode)
        {
            translations[getKey(source, srcLangCode, trgLangCode)] = target + " <stored translation>";
        }

        internal static bool TryTranslate(string source, string srcLangCode, string trgLangCode, out string storedTranslation)
        {
            return translations.TryGetValue(getKey(source, srcLangCode, trgLangCode), out storedTranslation);
        }

        private static string getKey(string source, string srcLangCode, string trgLangCode)
        {
            return source + srcLangCode + trgLangCode;
        }
    }
}
