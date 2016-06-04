using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.ClientApi;
using Rozmawiator.Data;
using Rozmawiator.Models;

namespace Rozmawiator.Extensions
{
    public static class PassiveConversationExtensions
    {
        public static IEnumerable<User> GetUsers(this PassiveConversation conversation)
        {
            return UserService.Users.Where(u => conversation.ParticipantsNicknames.Contains(u.Nickname));
        }
    }
}
