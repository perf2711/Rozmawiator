using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.RestClient.Helpers;
using Rozmawiator.RestClient.Models;

namespace Rozmawiator.RestClient
{
    public class ServerApi : BaseApi
    {
        public ServerApi(string baseUrl) : base(baseUrl)
        {
        }

        protected override string Get(Guid id) => $"/api/Servers/{id}";
        protected override string GetList() => "/api/Servers/List";
        protected string GetOnline() => "/api/Servers/Online";

        public async Task<HttpResponse> GetOnline(TokenModel token)
        {
            return await HttpHelper.Get(BaseUrl + GetOnline(), token);
        }
    }
}
