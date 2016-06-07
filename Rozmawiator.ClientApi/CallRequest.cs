using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Conversation;

namespace Rozmawiator.ClientApi
{
    public class CallRequest
    {
        public Conversation Conversation { get; }
        public Guid CallId { get; }
        public CallResponseType? Response { get; private set; }

        public CallRequest(Conversation conversation, Guid callId)
        {
            Conversation = conversation;
            CallId = callId;
        }

        public void Accept()
        {
            Response = CallResponseType.Accepted;
            Conversation.RespondToRequest(this);
        }

        public void Decline()
        {
            Response = CallResponseType.Denied;
            Conversation.RespondToRequest(this);
        }

        public void Ignore()
        {
            Response = null;
            Conversation.RespondToRequest(this);
        }
    }
}
