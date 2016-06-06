using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication
{
    public class Message
    {
        public static Message Create(Guid senderId)
        {
            return new Message { SenderId = senderId };
        }

        public static Message CreateRequest(Guid senderId, Guid requestId)
        {
            var message = new Message(true) {SenderId = senderId};
            return message.AddContent(requestId.ToByteArray());
        }

        public const int MaxLength = 65535;
        public static int HeaderLength { get; } = 17;

        protected Message(bool isRequestType = false)
        {
            IsRequestType = isRequestType;
        }

        private byte _messageType;

        public byte MessageType
        {
            get
            {
                if (IsRequestType)
                {
                    return (byte)(_messageType - 128);
                }
                return _messageType;
            }
            set
            {
                _messageType = value;
                if (IsRequestType)
                {
                    _messageType += 128;
                }
            }
        }

        public Guid SenderId { get; set; }
        public byte[] Content { get; set; }
        public bool IsRequestType { get; }

        public byte[] GetBytes()
        {
            var contentLength = Content?.Length ?? 0;
            var length = HeaderLength + contentLength;
            if (length > MaxLength)
            {
                length = MaxLength;
            }

            var result = new byte[length];
            var sender = SenderId.ToByteArray();

            // If content.Length + HeaderLength is more than MTU, return MTU - HeaderLength, and cap message length to MTU
            var contentToCopyLength = (contentLength + HeaderLength > MaxLength) ? MaxLength - HeaderLength : contentLength;

            Buffer.BlockCopy(sender, 0, result, 0, sender.Length);
            result[sender.Length] = MessageType;
            if (Content != null)
            {
                Buffer.BlockCopy(Content, 0, result, HeaderLength, contentToCopyLength);
            }

            return result;
        }

        public static Message FromBytes(byte[] msgBytes)
        {
            var type = msgBytes[0];
            var isRequest = (type & 128) == 128;
            if (isRequest)
            {
                return FromBytesRequest(msgBytes);
            }

            var sender = new byte[16];
            var content = new byte[msgBytes.Length - HeaderLength];

            Buffer.BlockCopy(msgBytes, 1, sender, 0, sender.Length);
            Buffer.BlockCopy(msgBytes, HeaderLength, content, 0, msgBytes.Length - HeaderLength);

            return ConstructMessage(type, sender, null, content);
        }

        private static Message FromBytesRequest(byte[] msgBytes)
        {
            var type = msgBytes[0];
            type -= 128;

            var sender = new byte[16];
            var request = new byte[16];
            var content = new byte[msgBytes.Length - sender.Length - request.Length];

            Buffer.BlockCopy(msgBytes, 1, sender, 0, sender.Length);
            Buffer.BlockCopy(msgBytes, 1 + request.Length, request, 0, request.Length);
            Buffer.BlockCopy(msgBytes, 1 + sender.Length + request.Length, content, 0, msgBytes.Length - 1 - sender.Length - request.Length);

            return ConstructMessage(type, sender, request, content);
        }

        private static Message ConstructMessage(byte type, byte[] sender, byte[] request, byte[] content)
        {
            var message = new Message(request != null)
            {
                MessageType = type,
                SenderId = new Guid(sender)
            };

            if (request != null)
            {
                message.AddContent(request);
            }
            return message.AddContent(content);
        }
    }
}
