using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Communication.Server;

namespace Rozmawiator.Request
{
    public class ExpectedMessage
    { 
        public byte MessageType { get; set; }

        public event Action<ExpectedMessage, Message> Arrived;

        public ExpectedMessage(byte type, Action<ExpectedMessage, Message> callback = null)
        {
            MessageType = type;
            Arrived = callback;
        }

        public ExpectedMessage(ServerMessageType type, Action<ExpectedMessage, Message> callback = null) : this((byte)type, callback)
        {
        }

        public ExpectedMessage(ConversationMessageType type, Action<ExpectedMessage, Message> callback = null) : this((byte)type, callback)
        {
        }

        public ExpectedMessage(CallMessageType type, Action<ExpectedMessage, Message> callback = null) : this((byte)type, callback)
        {
        }

        public bool Test(Message message)
        {
            if (MessageType != message.MessageType)
            {
                return false;
            }

            Arrived?.Invoke(this, message);
            return true;
        }
    }
}
