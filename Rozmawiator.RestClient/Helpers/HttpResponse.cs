using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rozmawiator.RestClient.Errors;

namespace Rozmawiator.RestClient.Helpers
{
    public class HttpResponse
    {
        public HttpStatusCode ResponseCode { get; }
        public string ResponseString { get; }
        public bool IsSuccessStatusCode => ResponseCode == HttpStatusCode.OK;

        public HttpResponse(HttpStatusCode responseCode, string responseString)
        {
            ResponseCode = responseCode;
            ResponseString = responseString;
        }

        public Dictionary<string, object> GetModel()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(ResponseString);
        }

        public T GetModel<T>()
        {
            return JsonConvert.DeserializeObject<T>(ResponseString);
        }

        public IRestError GetError()
        {
            IRestError error;
            if (AuthenticationError.TryGetModel(GetModel(), out error))
            {
                return error;
            }
            if (ModelStateError.TryGetModel(GetModel(), out error))
            {
                return error;
            }

            return null;
        }
    }
}
