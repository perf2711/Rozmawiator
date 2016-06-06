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
        public Guid[] Participants { get; set; }
    }
}
