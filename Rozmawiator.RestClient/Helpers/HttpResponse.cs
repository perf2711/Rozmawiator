using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
    }
}
