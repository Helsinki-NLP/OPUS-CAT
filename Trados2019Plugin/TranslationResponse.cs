using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatTranslationProvider
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class TranslationResponseText
    {
        public string content { get; set; }
    }

    public class TranslationResponse
    {
        public string type { get; set; }
        public List<TranslationResponseText> texts { get; set; }
    }

    public class TranslationResponseRoot
    {
        public TranslationResponse response { get; set; }
    }
}
