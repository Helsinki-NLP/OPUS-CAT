using System.Web.Http;

namespace OpusCatMTEngine
{
    public class MachineTranslationController : ApiController
    {
        private readonly IMtProvider mtProvider;

        public MachineTranslationController(IMtProvider mtProvider)
        {
            this.mtProvider = mtProvider;
        }
        
        public string Get(string tokenCode, string input, string srcLangCode, string trgLangCode, string modelTag)
        {
            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);
            return this.mtProvider.Translate(input, sourceLang, targetLang, modelTag).Result.Translation;
        }
    }
}
