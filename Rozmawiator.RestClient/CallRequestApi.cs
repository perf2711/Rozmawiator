using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.RestClient
{
    public class CallRequestApi : BaseApi
    {
        public CallRequestApi(string baseUrl) : base(baseUrl)
        { 
        }

        protected override string Get(Guid id) => $"/api/CallRequests/{id}";
        protected override string GetList() => "/api/CallRequests/List";
    }
}
