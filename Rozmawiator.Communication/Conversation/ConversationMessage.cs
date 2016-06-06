using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Conversation
{
    public class ConversationMessage : Message
    {
        public ConversationMessageType Type
        {
            get { return (ConversationMessageType)MessageType; }
            set { MessageType = (byte)value; }
        }

        public static ConversationMessage Create(Guid senderId, Guid conversationId)
        {
            return (ConversationMessage) Create(senderId).AddContent(conversationId.ToByteArray());
        }

        public static ConversationMessage CreateRequest(Guid senderId, Guid requestId, Guid conversationId)
        {
            return (ConversationMessage) CreateRequest(senderId, requestId).AddContent(conversationId.ToByteArray());
        }

        public override byte[] Content => base.Content.Skip(16).ToArray();

        public Guid GetConversationId()
        {
            return new Guid(base.Content.Take(16).ToArray());
        }
    }
}
