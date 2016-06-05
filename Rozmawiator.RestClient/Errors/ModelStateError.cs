using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Rozmawiator.RestClient.Errors
{
    public class ModelStateError : IRestError
    {
        public class FieldError
        {
            public string Name { get; }
            public string[] Errors { get; }

            public FieldError(string name, params string[] errors)
            {
                Name = name;
                Errors = errors;
            }
        }

        public string Message { get; }
        public FieldError[] FieldErrors { get; }

        public ModelStateError(string message, params FieldError[] fieldErrors)
        {
            Message = message;
            FieldErrors = fieldErrors;
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
            var message = json["Message"] as string;
            var rawFieldErrors = json["ModelState"] as JObject;
            if (message == null || rawFieldErrors == null)
            {
                throw new InvalidOperationException("Failed to get model.");
            }

            var fieldErrors = new List<FieldError>(rawFieldErrors.Count);
            foreach (var rawFieldError in rawFieldErrors)
            {
                var errors = rawFieldError.Value.Select(v => v.ToString());

                fieldErrors.Add(new FieldError(rawFieldError.Key, errors.ToArray()));
            }

            return new ModelStateError(message, fieldErrors.ToArray());
        }

        public override string ToString()
        {
            return FieldErrors.SelectMany(f => f.Errors).Aggregate((a, b) => a + "\n" + b);
        }
    }
}
