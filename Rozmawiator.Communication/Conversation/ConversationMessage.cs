using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Conversation
{
    public class ConversationMessage : Message
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

        public static ConversationMessage Create(Guid senderId, byte[] content)
        {
            return (ConversationMessage) new ConversationMessage(senderId).AddContent(content);
        }

        public override byte[] Content
        {
            get { return RawContent?.Skip(16).ToArray(); }
            set { RawContent = value; }
        }

        public Guid GetConversationId()
        {
            return new Guid(RawContent.Take(16).ToArray());
        }
    }
}
