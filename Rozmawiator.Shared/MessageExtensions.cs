using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Shared
{
    public static class MessageExtensions
    {
        /// <summary>
        /// Sets message receiver id.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="id">Id to set</param>
        /// <returns></returns>
        public static Message Receiver(this Message message, short id)
        {
            message.Receiver = id;
            return message;
        }
        
        /// <summary>
        /// Sets message sender id.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="id">Id to set</param>
        /// <returns></returns>
        public static Message Sender(this Message message, short id)
        {
            message.Sender = id;
            return message;
        }

        /// <summary>
        /// Sets message type.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="type">Type to set</param>
        /// <returns></returns>
        public static Message Type(this Message message, Message.MessageType type)
        {
            message.Type = type;
            return message;
        }

        /// <summary>
        /// Sets message content.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="content">Byte content to set</param>
        /// <returns></returns>
        public static Message Content(this Message message, byte[] content)
        {
            message.Content = content;
            return message;
        }

        /// <summary>
        /// Sets message content. Message will be encoded in Unicode.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="content">String content to set.</param>
        /// <returns></returns>
        public static Message Content(this Message message, string content)
        {
            message.SetTextContent(content);
            return message;
        }

        /// <summary>
        /// Client sends this message with its nickname to server on connection.
        /// Server sends this message on client connection as response.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="nickname">Client's nickname</param>
        /// <returns></returns>
        public static Message Hello(this Message message, string nickname)
        {
            return message.Type(Message.MessageType.Hello).Content(nickname);
        }

        /// <summary>
        /// Client sends this message to server on disconnect.
        /// Server sends this message on client disconnect caused by server.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="reason">Optional reason for disconnect</param>
        /// <returns></returns>
        public static Message Bye(this Message message, string reason)
        {
            return message.Type(Message.MessageType.Bye).Content(reason);
        }

        /// <summary>
        /// Client sends this message to avoid timeout.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <returns></returns>
        public static Message KeepAlive(this Message message)
        {
            return message.Type(Message.MessageType.KeepAlive).Content("");
        }

        /// <summary>
        /// Standard text message.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="content">Message text content</param>
        /// <returns></returns>
        public static Message Text(this Message message, string content)
        {
            return message.Type(Message.MessageType.Text).Content(content);
        }

        /// <summary>
        /// Audio message. Content should be already encoded.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="samples">Encoded audio samples</param>
        /// <returns></returns>
        public static Message Audio(this Message message, byte[] samples)
        {
            return message.Type(Message.MessageType.Audio).Content(samples);
        }

        /// <summary>
        /// Client sends this message when it wants to call somebody.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="nickname">Nickname of called user</param>
        /// <returns></returns>
        public static Message Call(this Message message, string nickname)
        {
            return message.Type(Message.MessageType.Call).Content(nickname);
        }
        
        /// <summary>
        /// Server sends this message to called client. Client should respond with CallResponse.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="nickname">Nickname of calling user</param>
        /// <returns></returns>
        public static Message CallRequest(this Message message, string nickname)
        {
            return message.Type(Message.MessageType.CallRequest).Content(nickname);
        }

        /// <summary>
        /// Client sends this message to calling client, carrying its response to call request. 
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="callerId">Calling client's ID</param>
        /// <param name="responseType">Response to call</param>
        /// <returns></returns>
        public static Message CallResponse(this Message message, short callerId, Message.CallResponseType responseType)
        {
            var content = new[] {(byte) responseType};
            return message.Type(Message.MessageType.CallResponse).Content(content).Receiver(callerId);
        }

        /// <summary>
        /// Client sends this message when it joins a conversation. Other clients should respond with this message as well.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="nickname">Client's nickname</param>
        /// <returns></returns>
        public static Message HelloConversation(this Message message, string nickname)
        {
            return message.Type(Message.MessageType.HelloConversation).Content(nickname);
        }

        /// <summary>
        /// Client sends this message when it leaves the conversation.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="reason">Optional reason</param>
        /// <returns></returns>
        public static Message ByeConversation(this Message message, string reason)
        {
            return message.Type(Message.MessageType.ByeConversation).Content(reason);
        }

        /// <summary>
        /// Server sends this message to clients in conversation to signify that the conversation has been closed.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="reason">Optional reason</param>
        /// <returns></returns>
        public static Message CloseConversation(this Message message, string reason)
        {
            return message.Type(Message.MessageType.CloseConversation).Content(reason);
        }

        /// <summary>
        /// Conversation-less message.
        /// </summary>
        /// <param name="message">Message to modify</param>
        /// <param name="nickname">When sending, this should be Receiver's nickname. On the receiving side, this should be Sender's nickname.</param>
        /// <param name="content">Message content</param>
        /// <returns></returns>
        public static Message DirectText(this Message message, string nickname, string content)
        {   
            var nicknameBytes = Encoding.Unicode.GetBytes(nickname);
            var contentBytes = Encoding.Unicode.GetBytes(content);

            var bytes = nicknameBytes.Concat(new byte[] {0x0, 0x0}).Concat(contentBytes).ToArray();
            return message.Type(Message.MessageType.DirectText).Content(bytes);
        }
    }
}
