using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication.Conversation;

namespace Rozmawiator.Communication.Call
{
    public enum CallMessageType
    {
        NewUser = ConversationMessageType.CallResponse + 1,
        UserDeclined,
        Bye,
        UserLeft,
        Audio
    }
}
