using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.Entities;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.RestClient.Errors;
using CallRequest = Rozmawiator.Models.CallRequest;
using Conversation = Rozmawiator.Models.Conversation;

namespace Rozmawiator.Data
{
    public static class CallService
    {
        public static List<CallRequest> CallRequests { get; set; } = new List<CallRequest>();

        public static Call CurrentCall { get; set; }
    }
}
