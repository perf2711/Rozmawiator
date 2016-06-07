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

        public event Action<ExpectedMessage, IMessage> Arrived;

        public ExpectedMessage(byte type, Action<ExpectedMessage, IMessage> callback = null)
        {
            MessageType = type;
            Arrived = callback;
        }

        public ExpectedMessage(ServerMessageType type, Action<ExpectedMessage, IMessage> callback = null) : this((byte)type, callback)
        {
        }

        public ExpectedMessage(ConversationMessageType type, Action<ExpectedMessage, IMessage> callback = null) : this((byte)type, callback)
        {
        }

        public ExpectedMessage(CallMessageType type, Action<ExpectedMessage, IMessage> callback = null) : this((byte)type, callback)
        {
        }

        public bool Test(IMessage message)
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
