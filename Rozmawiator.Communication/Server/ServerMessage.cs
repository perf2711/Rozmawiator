using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Server
{
    public class ServerMessage : Message
    {
        protected ServerMessage(Guid senderId)
        {
            SenderId = senderId;
        }

        public ServerMessageType Type 
        {
            get { return (ServerMessageType) MessageType; }
            set { MessageType = (byte) value; }
        }

        public new static ServerMessage Create(Guid senderId)
        {
            return new ServerMessage(senderId);
        }

        public static ServerMessage Create(Guid senderId, byte[] content)
        {
            return (ServerMessage) new ServerMessage(senderId).AddContent(content);
        }
    }
}
