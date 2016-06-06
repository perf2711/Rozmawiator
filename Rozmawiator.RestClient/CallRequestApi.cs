using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.RestClient.Helpers;
using Rozmawiator.RestClient.Models;

namespace Rozmawiator.RestClient
{
    public class CallRequestApi : BaseApi
    {
        public CallRequestApi(string baseUrl) : base(baseUrl)
        { 
        }

        protected override string Get(Guid id) => $"/api/CallRequests/{id}";
        protected override string GetList() => "/api/CallRequests/List";

        public async Task<HttpResponse> GetConversation(TokenModel token, string caller)
        {
            var url = $"/api/CallRequests/Conversation/{caller}";
            return await HttpHelper.Get(BaseUrl + url, token);
        }
    }
}
