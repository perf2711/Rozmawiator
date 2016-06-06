using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using Rozmawiator.Database.Entities;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.Models;
using Rozmawiator.RestClient.Errors;
using Rozmawiator.RestClient.Helpers;
using Conversation = Rozmawiator.Models.Conversation;

namespace Rozmawiator.Data
{
    public static class PassiveConversationService
    {
        public static List<Conversation> Conversations { get; set; }

        public static async Task UpdateConversations()
        {
            var filter = Filter.CreateNew.Set("Type", (int) ConversationType.Passive);
            var response = await RestService.ConversationApi.Get(RestService.CurrentToken, filter);

            if (response.Error != null)
            {
                throw new RestErrorException(response.Error);
            }

            var conversations = response.GetModel<ConversationViewModel[]>();

            await UserService.AddUsers(conversations.SelectMany(c => c.Participants).ToArray());
            Conversations = conversations.Select(c => new Conversation
            {
                Id = c.Id,
                Type = c.Type,
                Creator = UserService.Users.FirstOrDefault(u => u.Nickname == c.Creator),
                Owner = UserService.Users.FirstOrDefault(u => u.Nickname == c.Owner),
                Participants = UserService.Users.Where(u => c.Participants.Contains(u.Nickname)).ToArray()
            }).ToList();
        }

        public static async Task<Conversation> AddConversation(params string[] participants)
        {
            await UserService.AddUsers(participants);
            var conversation = new Conversation
            {
                Id = Guid.Empty,
                Type = ConversationType.Passive,
                Creator = null,
                Owner = null,
                Participants = UserService.Users.Where(u => participants.Contains(u.Nickname)).ToArray()
            };

            Conversations.Add(conversation);
            return conversation;
        }

        public static async Task GetMoreMessages(this Conversation conversation, int page = 0, int count = 100)
        {
            var filter = Filter.CreateNew.Set("ConversationId", conversation.Id);
            HttpResponse response;
            if (conversation.Messages.Any())
            {
                var earliestMessage = conversation.Messages.MinBy(m => m.Timestamp);
                response =
                    await RestService.MessageApi.Get(RestService.CurrentToken, filter, earliestMessage.Id, page, count);
            }
            else
            {
                response =
                    await RestService.MessageApi.Get(RestService.CurrentToken, filter, page, count);
            }

            if (response.Error != null)
            {
                throw new RestErrorException(response.Error);
            }

            var messages = response.GetModel<MessageViewModel[]>();
            conversation.Messages.AddRange(
                messages.Select(
                    m =>
                        new TextMessage(m.Id, m.Content, m.Timestamp,
                            UserService.Users.FirstOrDefault(u => u.Nickname == m.Sender))));
        }
    }
}
