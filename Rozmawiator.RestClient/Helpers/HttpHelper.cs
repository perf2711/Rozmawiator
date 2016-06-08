using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rozmawiator.RestClient.Errors;
using Rozmawiator.RestClient.Models;

namespace Rozmawiator.RestClient.Helpers
{
    public static class HttpHelper
    {
        public static async Task<HttpResponse> Get(string url, TokenModel token = null)
        {
            using (var client = new HttpClient())
            {
                if (token != null)
                {
                    client.DefaultRequestHeaders.Authorization = token.GetHeader();
                }

                HttpResponseMessage response;
                HttpStatusCode responseCode;
                try
                {
                    response = await client.GetAsync(url);
                }
                catch (HttpRequestException e)
                {
                    responseCode = HttpStatusCode.Gone;
                    return new HttpResponse(responseCode, new UnavailableError { Error = e.Message });
                }
                responseCode = response.StatusCode;
                var responseContent = response.Content;
                var mediaType = response.Content?.Headers?.ContentType?.MediaType;

                return mediaType == "application/json"
                    ? new HttpResponse(responseCode, await responseContent.ReadAsStringAsync(), mediaType)
                    : new HttpResponse(responseCode, await responseContent.ReadAsByteArrayAsync(), mediaType);
            }
        }

        public static async Task<HttpResponse> Post(string url, object model, TokenModel token = null)
        {
            var data = JsonConvert.SerializeObject(model);
            var jsonContent = new StringContent(data, Encoding.Unicode, "application/json");
            return await Post(url, jsonContent, "application/json", token);
        }

        public static async Task<HttpResponse> Post(string url, HttpContent content, TokenModel token = null)
        {
            using (var client = new HttpClient())
            {
                if (token != null)
                {
                    client.DefaultRequestHeaders.Authorization = token.GetHeader();
                }

                HttpResponseMessage response;
                HttpStatusCode responseCode;
                try
                {
                    response = await client.PostAsync(url, content);
                }
                catch (HttpRequestException e)
                {
                    responseCode = HttpStatusCode.Gone;
                    return new HttpResponse(responseCode, new UnavailableError {Error = e.Message});
                }
                responseCode = response.StatusCode;
                var responseContent = response.Content;
                var mediaType = response.Content?.Headers?.ContentType?.MediaType;

                return mediaType == "application/json"
                    ? new HttpResponse(responseCode, await responseContent.ReadAsStringAsync(), mediaType)
                    : new HttpResponse(responseCode, await responseContent.ReadAsByteArrayAsync(), mediaType);
            }
        }

        public static async Task<HttpResponse> Post(string url, HttpContent content, string contentType, TokenModel token = null)
        {
            using (var client = new HttpClient())
            {
                if (token != null)
                {
                    client.DefaultRequestHeaders.Authorization = token.GetHeader();
                }

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

                HttpResponseMessage response;
                HttpStatusCode responseCode;
                try
                {
                    response = await client.PostAsync(url, content);
                }
                catch (HttpRequestException e)
                {
                    responseCode = HttpStatusCode.Gone;
                    return new HttpResponse(responseCode, new UnavailableError { Error = e.Message });
                }
                responseCode = response.StatusCode;
                var responseContent = response.Content;
                var mediaType = response.Content?.Headers?.ContentType?.MediaType;

                return mediaType == "application/json"
                    ? new HttpResponse(responseCode, await responseContent.ReadAsStringAsync(), mediaType)
                    : new HttpResponse(responseCode, await responseContent.ReadAsByteArrayAsync(), mediaType);
            }
        }
    }
}