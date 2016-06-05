using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.RestClient
{
    public class MessageApi : BaseApi
    {
        public MessageApi(string baseUrl) : base(baseUrl)
        {
        }

        protected override string Get(Guid id) => $"/api/Messages/{id}";
        protected override string GetList() => "/api/Messages/List";
    }
}
