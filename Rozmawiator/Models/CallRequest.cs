using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Database.Entities;

namespace Rozmawiator.Models
{
    public class CallRequest
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Conversation Conversation { get; set; }
        public ClientApi.CallRequest ClientCallRequest { get; set; }

        public CallResponseType State { get; set; }
    }
}
