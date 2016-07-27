using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.RestClient;
using Rozmawiator.RestClient.Models;

namespace Rozmawiator.Data
{
    public static class RestService
    {
        public static class ErrorParser
        {
            
        }

        public static string BaseUrl = Properties.Settings.Default.RestUrl;

        public static TokenModel CurrentToken { get; set; }

        public static CallRequestApi CallRequestApi { get; } = new CallRequestApi(BaseUrl);
        public static ConversationApi ConversationApi { get; } = new ConversationApi(BaseUrl);
        public static MessageApi MessageApi { get; } = new MessageApi(BaseUrl);
        public static ServerApi ServerApi { get; } = new ServerApi(BaseUrl);
        public static UserApi UserApi { get; } = new UserApi(BaseUrl);
    }
}
