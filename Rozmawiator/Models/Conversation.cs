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
        public List<User> Participants { get; set; }
        public List<TextMessage> Messages { get; } = new List<TextMessage>();
    }
}
