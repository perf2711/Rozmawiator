using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.RestClient.Errors
{
    public class AuthenticationError : IRestError
    {
        public string Error { get; }
        public string Description { get; }

        public AuthenticationError(string error, string description)
        {
            Error = error;
            Description = description;
        }

        public static bool TryGetModel(Dictionary<string, object> json, out IRestError model)
        {
            try
            {
                model = GetModel(json);
                return true;
            }
            catch (Exception)
            {
                model = null;
                return false;
            }
        }

        public static IRestError GetModel(Dictionary<string, object> json)
        {
            var error = json["error"] as string;
            var description = json["error_description"] as string;

            return new AuthenticationError(error, description);
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
