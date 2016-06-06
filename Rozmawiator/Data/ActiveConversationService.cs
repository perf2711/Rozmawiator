using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.Entities;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.RestClient.Errors;
using Conversation = Rozmawiator.Models.Conversation;

namespace Rozmawiator.Data
{
    public static class ActiveConversationService
    {
        public static List<Conversation> Conversations { get; set; }

        public static Conversation CurrentActiveConversation { get; set; }

        public static async Task UpdateConversations()
        {
            var filter = Filter.CreateNew.Set("Type", (int)ConversationType.Active);
            var response = await RestService.ConversationApi.Get(RestService.CurrentToken, filter);

            if (response.Error != null)
            {
                throw new RestErrorException(response.Error);
            }

            var conversations = response.GetModel<ConversationViewModel[]>();

            await UserService.AddUsers(conversations.SelectMany(c => c.Participants).ToArray());
            await UserService.AddUsers(conversations.Where(u => !string.IsNullOrEmpty(u.Creator)).Select(u => u.Creator).ToArray());
            await UserService.AddUsers(conversations.Where(u => !string.IsNullOrEmpty(u.Owner)).Select(u => u.Owner).ToArray());

            Conversations = conversations.Select(c => new Conversation
            {
                Id = c.Id,
                Type = c.Type,
                Creator = UserService.Users.FirstOrDefault(u => u.Nickname == c.Creator),
                Owner = UserService.Users.FirstOrDefault(u => u.Nickname == c.Owner),
                Participants = UserService.Users.Where(u => c.Participants.Contains(u.Nickname)).ToArray()
            }).ToList();
        }
    }
}
