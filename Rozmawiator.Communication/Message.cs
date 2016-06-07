using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Communication.Server;

namespace Rozmawiator.Communication
{
    public class Message : IMessage
    {
        public const int MaxLength = 65535;
        public static int HeaderLength { get; } = 17;

        protected Message()
        {
        }

        public static IMessage Create(Guid senderId)
        {
            return new Message {SenderId = senderId};
        }

        public Guid SenderId { get; set; }
        public byte MessageType { get; set; }
        public byte[] RawContent { get; set; }
        public virtual byte[] Content
        {
            get { return RawContent; }
            set { RawContent = value; }
        }

        public MessageCategory Category => GetCategory(MessageType);

        public static MessageCategory GetCategory(byte messageType)
        {
            if (Enum.IsDefined(typeof(ServerMessageType), (int)messageType))
            {
                return MessageCategory.Server;
            }
            if (Enum.IsDefined(typeof(ConversationMessageType), (int)messageType))
            {
                return MessageCategory.Conversation;
            }
            if (Enum.IsDefined(typeof(CallMessageType), (int)messageType))
            {
                return MessageCategory.Call;
            }
            throw new IndexOutOfRangeException();
        }

        public byte[] GetBytes()
        {
            var contentLength = RawContent?.Length ?? 0;
            var length = HeaderLength + contentLength;
            if (length > MaxLength)
            {
                length = MaxLength;
            }

            var result = new byte[length];
            var sender = SenderId.ToByteArray();

            // If content.Length + HeaderLength is more than MTU, return MTU - HeaderLength, and cap message length to MTU
            var contentToCopyLength = (contentLength + HeaderLength > MaxLength) ? MaxLength - HeaderLength : contentLength;

            result[0] = MessageType;
            Buffer.BlockCopy(sender, 0, result, 1, sender.Length);
            if (RawContent != null)
            {
                Buffer.BlockCopy(RawContent, 0, result, HeaderLength, contentToCopyLength);
            }

            return result;
        }

        public static IMessage FromBytes(byte[] msgBytes)
        {
            var type = msgBytes[0];

            var sender = new byte[16];
            var content = new byte[msgBytes.Length - HeaderLength];

            Buffer.BlockCopy(msgBytes, 1, sender, 0, sender.Length);
            Buffer.BlockCopy(msgBytes, HeaderLength, content, 0, msgBytes.Length - HeaderLength);

            return ConstructMessage(type, sender, content);
        }

        private static IMessage ConstructMessage(byte type, byte[] sender, byte[] content)
        {
            IMessage message;
            switch (GetCategory(type))
            {
                case MessageCategory.Server:
                    message = ServerMessage.Create(new Guid(sender), content);
                    message.MessageType = type;
                    break;
                case MessageCategory.Conversation:
                    if (content.Length < 16)
                    {
                        throw new InvalidOperationException("Content must contain conversation ID.");
                    }
                    message = ConversationMessage.Create(new Guid(sender), content);
                    message.MessageType = type;
                    break;
                case MessageCategory.Call:
                    if (content.Length < 16)
                    {
                        throw new InvalidOperationException("Content must contain conversation ID.");
                    }
                    message = CallMessage.Create(new Guid(sender), content);
                    message.MessageType = type;
                    break;
                default:
                    return null;
            }
            return message;
            //return message.AddContent(content);
        }
    }
}
