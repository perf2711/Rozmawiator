using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Call
{
    public class CallMessage : Message, IMessage
    {
        protected CallMessage(Guid senderId)
        {
            SenderId = senderId;
        }

        public CallMessageType Type
        {
            get { return (CallMessageType)MessageType; }
            set { MessageType = (byte)value; }
        }

        public static CallMessage Create(Guid senderId, Guid callId)
        {
            return (CallMessage) new CallMessage(senderId).AddContent(callId.ToByteArray());
        }

        public override byte[] Content => base.Content.Skip(16).ToArray();

        public Guid GetCallId()
        {
            return new Guid(base.Content.Take(16).ToArray());
        }
    }
}
