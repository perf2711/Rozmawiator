using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.Models;
using Rozmawiator.RestClient.Errors;

namespace Rozmawiator.Data
{
    public static class CallService
    {
        public static List<CallRequest> CallRequests { get; set; } = new List<CallRequest>();

        public static Call CurrentCall { get; set; }
    }
}
