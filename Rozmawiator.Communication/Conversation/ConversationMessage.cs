using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Conversation
{
    public class ConversationMessage : Message, IMessage
    {
        protected ConversationMessage(Guid senderId)
        {
            SenderId = senderId;
        }

        public ConversationMessageType Type
        {
            get { return (ConversationMessageType)MessageType; }
            set { MessageType = (byte)value; }
        }

        public static ConversationMessage Create(Guid senderId, Guid conversationId)
        {
            return (ConversationMessage) new ConversationMessage(senderId).AddContent(conversationId.ToByteArray());
        }

        public override byte[] Content => base.Content.Skip(16).ToArray();

        public Guid GetConversationId()
        {
            return new Guid(base.Content.Take(16).ToArray());
        }
    }
}
