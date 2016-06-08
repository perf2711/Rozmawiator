using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.RestClient.Errors
{
    public class UnavailableError : IRestError
    {
        public string Error { get; set; }

        public override string ToString()
        {
            return "Serwer logowania jest niedostępny.";
        }
    }
}
