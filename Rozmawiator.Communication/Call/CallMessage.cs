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

        public static CallMessage Create(Guid senderId, byte[] content)
        {
            return (CallMessage)new CallMessage(senderId).AddContent(content);
        }

        public override byte[] Content
        {
            get { return RawContent?.Skip(16).ToArray(); }
            set { RawContent = value; }
        }

        public Guid GetCallId()
        {
            return new Guid(RawContent.Take(16).ToArray());
        }
    }
}
