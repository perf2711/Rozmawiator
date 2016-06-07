using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Models
{
    public class Call
    {
        public Guid Id { get; set; }
        public List<User> Participants { get; set; }
        public Conversation Conversation { get; set; }
    }
}
