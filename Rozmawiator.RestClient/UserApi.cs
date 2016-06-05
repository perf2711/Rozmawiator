using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.RestClient.Helpers;
using Rozmawiator.RestClient.Models;

namespace Rozmawiator.RestClient
{
    public class UserApi : BaseApi
    {
        public UserApi(string baseUrl) : base(baseUrl)
        {
        }

        protected override string Get(Guid id) => $"/api/Users/{id}";
        protected override string GetList() => "/api/Users/List";

        public async Task<HttpResponse> GetLogged(TokenModel token)
        {
            const string url = "/api/Users";
            return await HttpHelper.Get(BaseUrl + url, token);
        }

        public async Task<HttpResponse> Register(RegisterViewModel viewModel)
        {
            const string url = "/api/Account/Register";
            return await HttpHelper.Post(BaseUrl + url, viewModel);
        }

        public async Task<HttpResponse> Login(string username, string password)
        {
            const string url = "/token";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("grant_type", "password"),
            });
            return await HttpHelper.Post(BaseUrl + url, content);
        }

        public async Task<HttpResponse> GetAvatar(TokenModel token, string username)
        {
            var url = $"/api/Users/Avatar/{username}";
            return await HttpHelper.Get(BaseUrl + url, token);
        }

        public async Task<HttpResponse> SetAvatar(TokenModel token)
        {
            throw new NotImplementedException();
        }
    }
}
