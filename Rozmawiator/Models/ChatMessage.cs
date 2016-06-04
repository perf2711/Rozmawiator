using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rozmawiator.Models
{
    public class ChatMessage
    {
        public User Sender { get; protected set; }
        public DateTime Timestamp { get; protected set; }
        public string Content { get; protected set; }

        public ChatMessage(string content, DateTime timestamp, User sender) : this(content, timestamp)
        {
            Sender = sender;
        }

        public ChatMessage(string content, DateTime timestamp)
        {
            Content = content;
            Timestamp = timestamp;
        }
    }
}
