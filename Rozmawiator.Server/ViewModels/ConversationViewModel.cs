using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Server.Api;

namespace Rozmawiator.Server.ViewModels
{
    public class ConversationViewModel
    {
        public Conversation Conversation { get; set; }

        public int Id => Conversation.Id;
        public string OwnerNickname => Conversation.Owner.Nickname;
        public int ClientCount => Conversation.Participants.Count();
    }
}
