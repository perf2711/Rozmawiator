using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.RestClient
{
    public class UserApi : BaseApi
    {
        public UserApi(string baseUrl) : base(baseUrl)
        {
        }

        protected override string Get(Guid id) => $"/api/Users/{id}";
        protected override string GetList() => "/api/Users/List";
    }
}
