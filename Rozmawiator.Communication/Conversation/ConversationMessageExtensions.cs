using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Conversation
{
    public static class ConversationMessageExtensions
    {
        public static ConversationMessage AddUser(this ConversationMessage message, Guid userId)
        {
            message.Type = ConversationMessageType.AddUser;
            return (ConversationMessage) message.AddContent(userId.ToByteArray());
        }

        public static ConversationMessage Bye(this ConversationMessage message, string reason)
        {
            message.Type = ConversationMessageType.Bye;
            return (ConversationMessage) message.AddContent(reason);
        }

        public static ConversationMessage NewUser(this ConversationMessage message, Guid userId)
        {
            message.Type = ConversationMessageType.NewUser;
            return (ConversationMessage)message.AddContent(userId.ToByteArray());
        }

        public static ConversationMessage UserLeft(this ConversationMessage message, Guid userId)
        {
            message.Type = ConversationMessageType.UserLeft;
            return (ConversationMessage)message.AddContent(userId.ToByteArray());
        }

        public static ConversationMessage Text(this ConversationMessage message, string content)
        {
            message.Type = ConversationMessageType.Text;
            return (ConversationMessage)message.AddContent(content);
        }

        public static ConversationMessage CreateCall(this ConversationMessage message)
        {
            message.Type = ConversationMessageType.CreateCall;
            return message;
        }

        /*
        public static ConversationMessage CallRequest(this ConversationMessage message, Guid userId)
        {
            message.Type = ConversationMessageType.CallRequest;
            return (ConversationMessage)message.AddContent(userId.ToByteArray());
        }
        */

        public static ConversationMessage CallRequest(this ConversationMessage message, Guid callId)
        {
            message.Type = ConversationMessageType.CallRequest;
            return (ConversationMessage) message.AddContent(callId.ToByteArray());
        }

        public static ConversationMessage RevokeCallRequest(this ConversationMessage message, Guid callId)
        {
            message.Type = ConversationMessageType.RevokeRequest;
            return (ConversationMessage) message.AddContent(callId.ToByteArray());
        }

        public static ConversationMessage CallResponse(this ConversationMessage message, CallResponseType callResponse)
        {
            message.Type = ConversationMessageType.CallResponse;
            return (ConversationMessage)message.AddContent((byte)callResponse);
        }
    }
}
