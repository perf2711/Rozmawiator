using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.RestClient.Errors
{
    /// <summary>
    /// Exception thrown when REST api returns an error code.
    /// </summary>
    public class RestErrorException : Exception
    {
        public IRestError Error { get; }

        public RestErrorException(IRestError error)
        {
            Error = error;
        }
    }
}
