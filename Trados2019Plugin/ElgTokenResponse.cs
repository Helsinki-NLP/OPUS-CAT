using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatTranslationProvider
{
    public class ElgTokenResponse
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }
        public string refresh_expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        [DeserializeAs(Name = "not-before-policy")]
        public string notBeforePolicy { get; set; }
        public string session_state { get; set; }
        public string scope { get; set; }
    }
}
