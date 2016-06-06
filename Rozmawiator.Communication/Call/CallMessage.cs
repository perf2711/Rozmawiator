using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Call
{
    public class CallMessage : Message
    {
        public CallMessageType Type
        {
            get { return (CallMessageType)MessageType; }
            set { MessageType = (byte)value; }
        }

        public static CallMessage Create(Guid senderId, Guid callId)
        {
            return (CallMessage) Create(senderId).AddContent(callId.ToByteArray());
        }

        public static CallMessage CreateRequest(Guid senderId, Guid requestId, Guid callId)
        {
            return (CallMessage) CreateRequest(senderId, requestId).AddContent(callId.ToByteArray());
        }

        public override byte[] Content => base.Content.Skip(16).ToArray();

        public Guid GetConversationId()
        {
            return new Guid(base.Content.Take(16).ToArray());
        }
    }
}
