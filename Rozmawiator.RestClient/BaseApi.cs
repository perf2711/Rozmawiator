using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.RestClient.Helpers;
using Rozmawiator.RestClient.Models;

namespace Rozmawiator.RestClient
{
    public class BaseApi
    {
        public string BaseUrl { get; }

        public BaseApi(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        protected virtual string Get(Guid id)
        {
            throw new NotSupportedException();
        }

        protected virtual string GetList()
        {
            throw new NotSupportedException();
        }

        public async Task<HttpResponse> Get(TokenModel token, Guid id)
        {
            return await HttpHelper.Get(BaseUrl + Get(id), token);
        }

        public async Task<HttpResponse> Get(TokenModel token, Filter filter)
        {
            return await HttpHelper.Post(BaseUrl + GetList(), filter.Filters, token);
        }
    }
}
