using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.Entities;

namespace Rozmawiator.Database.ViewModels
{
    public class ConversationViewModel
    {
        public Guid Id { get; set; }
        public ConversationType Type { get; set; }
        public string Owner { get; set; }
        public string Creator { get; set; }
        public string[] Participants { get; set; }
    }
}
