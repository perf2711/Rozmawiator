using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Database.ViewModels
{
    public class CallViewModel
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid ConversationId { get; set; }
        public Guid[] Participants { get; set; }
    }
}
