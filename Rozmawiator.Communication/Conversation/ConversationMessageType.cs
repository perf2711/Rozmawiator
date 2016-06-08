using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication.Server;

namespace Rozmawiator.Communication.Conversation
{
    public enum ConversationMessageType
    {
        AddUser = ServerMessageType.CreateConversation + 1,
        Bye,
        NewUser,
        UserLeft,
        Text,
        CreateCall,
        CallRequest,
        CallResponse,
        RevokeRequest
    }
}
