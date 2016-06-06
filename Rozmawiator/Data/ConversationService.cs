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
    public static class ConversationService
    {
        public static async Task<Conversation> AddConversation(ConversationType type, params string[] participants)
        {
            await UserService.AddUsers(participants);
            var conversation = new Conversation
            {
                Id = Guid.Empty,
                Type = type,
                Creator = null,
                Owner = null,
                Participants = UserService.Users.Where(u => participants.Contains(u.Nickname)).ToArray()
            };

            switch (type)
            {
                case ConversationType.Passive:
                    PassiveConversationService.Conversations.Add(conversation);
                    break;
                case ConversationType.Active:
                    break;
            }
            
            return conversation;
        }

        public static async Task<Conversation> AddConversation(ConversationViewModel model)
        {
            await UserService.AddUsers(model.Participants);
            if (!string.IsNullOrEmpty(model.Creator))
            {
                await UserService.AddUsers(model.Creator);
            }
            if (!string.IsNullOrEmpty(model.Owner))
            {
                await UserService.AddUsers(model.Owner);
            }
            var conversation = new Conversation
            {
                Id = model.Id,
                Type = model.Type,
                Creator = UserService.GetUser(model.Creator),
                Owner = UserService.GetUser(model.Owner),
                Participants = UserService.Users.Where(u => model.Participants.Contains(u.Nickname)).ToArray()
            };

            switch (model.Type)
            {
                case ConversationType.Passive:
                    PassiveConversationService.Conversations.Add(conversation);
                    break;
                case ConversationType.Active:
                    break;
            }

            return conversation;
        }

        public static async Task GetMoreMessages(this Conversation conversation, int page = 0, int count = 100)
        {
            var filter = Filter.CreateNew.Set("ConversationId", conversation.Id);
            HttpResponse response;
            if (conversation.Messages.Any())
            {
                var earliestMessage = conversation.Messages.MinBy(m => m.Timestamp);
                response = await RestService.MessageApi.Get(RestService.CurrentToken, filter, earliestMessage.Id, page, count);
            }
            else
            {
                response = await RestService.MessageApi.Get(RestService.CurrentToken, filter, page, count);
            }

            if (response.Error != null)
            {
                throw new RestErrorException(response.Error);
            }

            var messages = response.GetModel<MessageViewModel[]>();
            conversation.Messages.AddRange(messages.Select(m => new TextMessage(m.Id, m.Content, m.Timestamp, UserService.Users.FirstOrDefault(u => u.Nickname == m.Sender))));
        }
    }
}
