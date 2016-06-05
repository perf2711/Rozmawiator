using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rozmawiator.Models
{
    public class TextMessage
    {
        public Guid Id { get; protected set; }
        public User Sender { get; protected set; }
        public DateTime Timestamp { get; protected set; }
        public string Content { get; protected set; }

        public TextMessage(Guid id, string content, DateTime timestamp, User sender) : this(id, content, timestamp)
        {
            Sender = sender;
        }

        public TextMessage(Guid id, string content, DateTime timestamp)
        {
            Id = id;
            Content = content;
            Timestamp = timestamp;
        }
    }
}
