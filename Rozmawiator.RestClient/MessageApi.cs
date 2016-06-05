using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.RestClient.Helpers;
using Rozmawiator.RestClient.Models;

namespace Rozmawiator.RestClient
{
    public class MessageApi : BaseApi
    {
        public MessageApi(string baseUrl) : base(baseUrl)
        {
        }

        protected override string Get(Guid id) => $"/api/Messages/{id}";
        protected string GetList(int page, int count) => $"/api/Messages/List?page={page}&count={count}";
        protected string GetList(Guid startFrom, int page, int count) => $"/api/Messages/List?startFrom={startFrom}&page={page}&count={count}";

        public async Task<HttpResponse> Get(TokenModel token, Filter filter, int page = 0, int count = 100)
        {
            return await HttpHelper.Post(BaseUrl + GetList(page, count), filter.Filters, token);
        }

        public async Task<HttpResponse> Get(TokenModel token, Filter filter, Guid priorTo, int page = 0, int count = 100)
        {
            return await HttpHelper.Post(BaseUrl + GetList(priorTo, page, count), filter.Filters, token);
        }
    }
}
