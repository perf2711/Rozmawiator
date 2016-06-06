using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Server
{
    public class ServerMessage : Message
    {
        public ServerMessageType Type 
        {
            get { return (ServerMessageType) MessageType; }
            set { MessageType = (byte) value; }
        }

        public new static ServerMessage Create(Guid senderId)
        {
            return (ServerMessage) Message.Create(senderId);
        }

        public new static ServerMessage CreateRequest(Guid senderId, Guid requestId)
        {
            return (ServerMessage)Message.CreateRequest(senderId, requestId);
        }
    }
}
