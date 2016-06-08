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
        public object ResponseObject { get; }
        public string ContentType { get; }
        public bool IsSuccessStatusCode => ResponseCode == HttpStatusCode.OK;
        public IRestError Error { get; }

        public HttpResponse(HttpStatusCode responseCode, object responseObject, string contentType)
        {
            ResponseCode = responseCode;
            ResponseObject = responseObject;
            ContentType = contentType;

            if (ResponseCode != HttpStatusCode.OK && ContentType == "application/json")
            {
                Error = GetError();
            }
        }

        public HttpResponse(HttpStatusCode responseCode, IRestError error)
        {
            ResponseCode = responseCode;
            ResponseObject = null;
            ContentType = "";

            Error = error;
        }


        public Dictionary<string, object> GetModel()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(ResponseObject.ToString());
        }

        public T GetModel<T>()
        {
            return JsonConvert.DeserializeObject<T>(ResponseObject.ToString());
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

            return new UnknownError(GetModel());
        }
    }
}
