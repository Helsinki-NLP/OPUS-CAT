using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatTranslationProvider
{
    internal class ElgServiceHelper
    {
        internal static ElgTokenResponse GetAccessAndRefreshToken(string successCode)
        {
            var client = new RestClient("https://live.european-language-grid.eu/auth/realms/ELG/protocol/openid-connect/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("code", successCode);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("client_id", "elg-oob");
            request.AddParameter("redirect_uri", "urn:ietf:wg:oauth:2.0:oob");
            var response = client.Execute<ElgTokenResponse>(request);

            return response.Data;
        }
    }
}
