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
        private readonly Conversation _conversation;
        public Guid ConversationId { get; }
        public CallResponseType? Response { get; private set; }

        public CallRequest(Guid conversationId, Conversation conversation)
        {
            ConversationId = conversationId;
            _conversation = conversation;
        }

        public void Accept()
        {
            Response = CallResponseType.Accepted;
            _conversation.RespondToRequest(this);
        }

        public void Decline()
        {
            Response = CallResponseType.Denied;
            _conversation.RespondToRequest(this);
        }

        public void Ignore()
        {
            Response = null;
            _conversation.RespondToRequest(this);
        }
    }
}
