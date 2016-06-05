using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Rozmawiator.RestClient.Errors
{
    public class UnknownError : IRestError
    {
        public Dictionary<string, object> Content { get; }

        public UnknownError(Dictionary<string, object> content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return "Nieznany błąd.";
        }
    }
}
