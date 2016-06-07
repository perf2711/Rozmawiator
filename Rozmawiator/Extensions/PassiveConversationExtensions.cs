using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.ClientApi;
using Rozmawiator.Data;
using Rozmawiator.Models;
using Conversation = Rozmawiator.Models.Conversation;

namespace Rozmawiator.Extensions
{
    public static class PassiveConversationExtensions
    {
        public static IEnumerable<User> GetUsers(this Conversation conversation)
        {
            return UserService.Users.Where(u => conversation.Participants.Contains(u));
        }
    }
}
