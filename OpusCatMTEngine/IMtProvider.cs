
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    public interface IMtProvider
    {
        Task<TranslationPair> Translate(string inputString, IsoLanguage sourceLang, IsoLanguage targetLang, string modelTag);
    }
}
