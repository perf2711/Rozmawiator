using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Database.ViewModels
{
    public class MessageViewModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid SenderId { get; set; }
        public Guid ConversationId { get; set; }
    }
}
