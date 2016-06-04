using System;
using System.Linq;
using System.Text;

namespace Rozmawiator.Shared
{
    public class Message
    {
        public enum MessageType
        {
            Hello,
            Bye,
            KeepAlive,
            Text,
            Audio,
            Call,
            CallRequest,
            CallResponse,
            HelloConversation,
            ByeConversation,
            CloseConversation,
            DirectText
        }

        public enum CallResponseType
        {
            RequestDenied,
            RequestAccepted,
            TargetIsOffline,
            ExpiredCall
        }

        public enum MessageOrigin
        {
            Received,
            Sent
        }

        public const int Mtu = 65535;
        public const int HeaderLength = 5;

        public short Sender { get; set; }
        public short Receiver { get; set; }
        public MessageType Type { get; set; }
        public byte[] Content { get; set; }

        public DateTime Timestamp { get; }
        public MessageOrigin Origin { get; set; }

        public int Length => HeaderLength + Content.Length;

        public Message(short sender, short receiver, MessageType type, byte[] content)
        {
            Sender = sender;
            Receiver = receiver;
            Type = type;
            Content = content;

            Timestamp = DateTime.Now;
        }

        public Message(short sender, short receiver, MessageType type, string content)
        {
            Sender = sender;
            Receiver = receiver;
            Type = type;
            Content = Encoding.Unicode.GetBytes(content);
        }

        public Message(short receiver, MessageType type, byte[] content) : this(0, receiver, type, content)
        {
            
        }

        public Message(short receiver, MessageType type, string content) : this(0, receiver, type, content)
        {

        }

        public Message(MessageType type, byte[] content) : this(0, 0, type, content)
        {
            
        }

        public Message(MessageType type, string content) : this(0, 0, type, content)
        {
        }

        public Message() :this(0,0, MessageType.Text, "")
        {
            
        }

        public void SetTextContent(string content)
        {
            Content = Encoding.Unicode.GetBytes(content);
        }

        public string GetDirectTextNickname()
        {
            if (Type != MessageType.DirectText)
            {
                return null;
            }

            byte[] nicknameBytes = null;

            for (var i = 0; i < Content.Length; i+=2)
            {
                if (Content[i] == 0x0 && Content[i + 1] == 0x0)
                {
                    nicknameBytes = Content.Take(i).ToArray();
                }
            }

            return nicknameBytes == null ? null : Encoding.Unicode.GetString(nicknameBytes);
        }

        public string GetDirectTextContent()
        {
            byte[] textBytes = null;

            for (var i = 0; i < Content.Length; i+=2)
            {
                if (Content[i] == 0x0 && Content[i + 1] == 0x0)
                {
                    textBytes = Content.Skip(i + 2).ToArray();
                }
            }

            return textBytes == null ? null : Encoding.Unicode.GetString(textBytes);
        }

        public string GetTextContent()
        {
            if (Type == MessageType.DirectText)
            {
                return GetDirectTextContent();
            }
            return Encoding.Unicode.GetString(Content);
        }

        public byte[] GetBytes()
        {
            var contentLength = Content?.Length ?? 0;
            var length = HeaderLength + contentLength;
            if (length > Mtu)
            {
                length = Mtu;
            }

            var result = new byte[length];

            var sender = Sender.GetBytes();
            var receiver = Receiver.GetBytes();
            var type = (byte) Type;

            // If content.Length + HeaderLength is more than MTU, return MTU - HeaderLength, and cap message length to MTU
            var contentToCopyLength = (contentLength + HeaderLength > Mtu) ? Mtu - HeaderLength : contentLength;

            Buffer.BlockCopy(sender, 0, result, 0, sender.Length);
            Buffer.BlockCopy(receiver, 0, result, sender.Length, receiver.Length);
            result[sender.Length + receiver.Length] = type;
            if (Content != null)
            {
                Buffer.BlockCopy(Content, 0, result, HeaderLength, contentToCopyLength);
            }

            return result;
        }

        public static short GetSender(byte[] msgBytes)
        {
            var sender = new byte[2];
            sender[0] = msgBytes[0];
            sender[1] = msgBytes[1];
            return sender.GetShort();
        }

        public static short GetReceiver(byte[] msgBytes)
        {
            var receiver = new byte[2];
            receiver[0] = msgBytes[2];
            receiver[1] = msgBytes[3];
            return receiver.GetShort();
        }

        public static MessageType GetMessageType(byte[] msgBytes)
        {
            return (MessageType) msgBytes[4];
        }

        public static Message FromBytes(byte[] msgBytes)
        {
            var sender = new byte[2];
            var receiver = new byte[2];
            var type = (MessageType) msgBytes[4];

            var content = new byte[msgBytes.Length - HeaderLength];

            sender[0] = msgBytes[0];
            sender[1] = msgBytes[1];
            receiver[0] = msgBytes[2];
            receiver[1] = msgBytes[3];
            
            Buffer.BlockCopy(msgBytes, HeaderLength, content, 0, msgBytes.Length - HeaderLength);
            return new Message(sender.GetShort(), receiver.GetShort(), type, content);
        }

        public static Message Hello(string nickname)
        {
            var nickBytes = Encoding.Unicode.GetBytes(nickname);
            return new Message(-1, -1, MessageType.Hello, nickBytes);
        }

        public static Message Bye()
        {
            return new Message(MessageType.Bye, "");
        }

        public static Message KeepAlive()
        {
            return new Message(MessageType.KeepAlive, "");
        }

        public static Message Broadcast(MessageType type, byte[] content)
        {
            return new Message(type, content);
        }

        public static Message Broadcast(MessageType type, string content)
        {
            var msg = Encoding.Unicode.GetBytes(content);
            return Broadcast(MessageType.Text, msg);
        }

        public static Message To(short receiver, MessageType type, byte[] content)
        {
            return new Message(receiver, type, content);
        }

        public static Message To(short receiver, MessageType type, string content)
        {
            var msg = Encoding.Unicode.GetBytes(content);
            return To(receiver, type, msg);
        }

        public static Message To(short receiver, string content)
        {
            var msg = Encoding.Unicode.GetBytes(content);
            return To(receiver, MessageType.Text, msg);
        }

        public static Message CallResponse(short sender, short receiver, CallResponseType responseType)
        {
            var content = new [] {(byte)responseType};
            return new Message(sender, receiver, MessageType.CallResponse, content);
        }
    }
}
