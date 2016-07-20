using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication.Server
{
    public static class ServerMessageExtensions
    {
        public static ServerMessage Hello(this ServerMessage message)
        {
            message.Type = ServerMessageType.Hello;
            return message;
        }

        public static ServerMessage Bye(this ServerMessage message, string reason)
        {
            message.Type = ServerMessageType.Bye;
            return (ServerMessage)message.AddContent(reason);
        }

        public static ServerMessage KeepAlive(this ServerMessage message)
        {
            message.Type = ServerMessageType.KeepAlive;
            return message;
        }

        public static ServerMessage Ok(this ServerMessage message, byte[] data)
        {
            message.Type = ServerMessageType.Ok;
            return (ServerMessage) message.AddContent(data);
        }

        public static ServerMessage Ok(this ServerMessage message, string data)
        {
            var bytes = Encoding.Unicode.GetBytes(data);
            return message.Ok(bytes);
        }

        public static ServerMessage Error(this ServerMessage message, byte errorCode)
        {
            message.Type = ServerMessageType.Error;
            return (ServerMessage)message.AddContent(errorCode);
        }

        public static ServerMessage CreateConversation(this ServerMessage message)
        {
            message.Type = ServerMessageType.CreateConversation;
            return message;
        }
    }
}
