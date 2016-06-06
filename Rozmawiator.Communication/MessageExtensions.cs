using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication
{
    public static class MessageExtensions
    {
        public static Message AddContent(this Message message, byte[] content)
        {
            var buffer = new byte[message.Content.Length + content.Length];
            Buffer.BlockCopy(message.Content, 0, buffer, 0, message.Content.Length);
            Buffer.BlockCopy(content, 0, buffer, message.Content.Length, buffer.Length);
            message.Content = buffer;
            return message;
        }

        public static Message AddContent(this Message message, byte content)
        {
            var buffer = new byte[message.Content.Length + 1];
            Buffer.BlockCopy(message.Content, 0, buffer, 0, message.Content.Length);
            Buffer.BlockCopy(new [] {content}, 0, buffer, message.Content.Length, 1);
            message.Content = buffer;
            return message;
        }

        public static Message AddContent(this Message message, string content)
        {
            var bytes = Encoding.Unicode.GetBytes(content);
            return message.AddContent(bytes);
        }
    }
}
