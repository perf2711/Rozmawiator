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

        public Guid Id => Conversation.Id;
        public IEnumerable<ClientViewModel> Participants => Conversation.Participants.Select(c => new ClientViewModel {Client = c});
    }
}
