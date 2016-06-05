using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.Entities;

namespace Rozmawiator.Models
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public ConversationType Type { get; set; }
        public User Owner { get; set; }
        public User Creator { get; set; }
        public User[] Participants { get; set; }
    }
}
