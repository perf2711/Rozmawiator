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
            if (message.RawContent == null)
            {
                message.RawContent = content;
                return message;
            }

            var buffer = new byte[message.RawContent.Length + content.Length];
            Buffer.BlockCopy(message.RawContent, 0, buffer, 0, message.RawContent.Length);
            Buffer.BlockCopy(content, 0, buffer, message.RawContent.Length, content.Length);
            message.RawContent = buffer;
            return message;
        }

        public static IMessage AddContent(this IMessage message, byte content)
        {
            if (message.RawContent == null)
            {
                message.RawContent = new [] {content};
                return message;
            }

            var buffer = new byte[message.RawContent.Length + 1];
            Buffer.BlockCopy(message.RawContent, 0, buffer, 0, message.RawContent.Length);
            Buffer.BlockCopy(new [] {content}, 0, buffer, message.RawContent.Length, 1);
            message.RawContent = buffer;
            return message;
        }

        public static IMessage AddContent(this IMessage message, string content)
        {
            var bytes = Encoding.Unicode.GetBytes(content);
            return message.AddContent(bytes);
        }

        public static string GetStringContent(this Message message)
        {
            return message.Content == null 
                ? null 
                : Encoding.Unicode.GetString(message.Content);
        }

        public static Guid GetGuidContent(this Message message)
        {
            return message.Content == null
                ? Guid.Empty
                : new Guid(message.Content.Take(16).ToArray());
        }

        public static byte GetByteContent(this Message message)
        {
            return message.Content[0];
        }
    }
}
