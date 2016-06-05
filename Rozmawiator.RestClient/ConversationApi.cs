using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.RestClient
{
    public class ConversationApi : BaseApi
    {
        public ConversationApi(string baseUrl) : base(baseUrl)
        {
        }

        protected override string Get(Guid id) => $"/api/Conversations/{id}";
        protected override string GetList() => "/api/Conversations/List";
    }
}
