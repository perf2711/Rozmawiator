using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.RestClient.Models
{
    public class TokenModel
    {
        public string Access_Token { get; set; }
        public string Token_Type { get; set; }
        public string Username { get; set; }

        public AuthenticationHeaderValue GetHeader()
        {
            return new AuthenticationHeaderValue(Token_Type, Access_Token);
        }
    }
}
