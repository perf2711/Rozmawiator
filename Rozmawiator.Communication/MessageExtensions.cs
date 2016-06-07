using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Communication
{
    public static class MessageExtensions
    {
        public static IMessage AddContent(this IMessage message, byte[] content)
        {
            if (message.Content == null)
            {
                message.Content = content;
                return message;
            }

            var buffer = new byte[message.Content.Length + content.Length];
            Buffer.BlockCopy(message.Content, 0, buffer, 0, message.Content.Length);
            Buffer.BlockCopy(content, 0, buffer, message.Content.Length, buffer.Length);
            message.Content = buffer;
            return message;
        }

        public static IMessage AddContent(this IMessage message, byte content)
        {
            if (message.Content == null)
            {
                message.Content = new [] {content};
                return message;
            }

            var buffer = new byte[message.Content.Length + 1];
            Buffer.BlockCopy(message.Content, 0, buffer, 0, message.Content.Length);
            Buffer.BlockCopy(new [] {content}, 0, buffer, message.Content.Length, 1);
            message.Content = buffer;
            return message;
        }

        public static IMessage AddContent(this IMessage message, string content)
        {
            var bytes = Encoding.Unicode.GetBytes(content);
            return message.AddContent(bytes);
        }

        public static string GetStringContent(this Message message)
        {
            return Encoding.Unicode.GetString(message.Content);
        }

        public static Guid GetGuidContent(this Message message)
        {
            return new Guid(message.Content.Take(16).ToArray());
        }

        public static byte GetByteContent(this Message message)
        {
            return message.Content[0];
        }
    }
}
