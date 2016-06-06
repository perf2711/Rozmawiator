using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.Entities;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.RestClient.Errors;
using CallRequest = Rozmawiator.Models.CallRequest;
using Conversation = Rozmawiator.Models.Conversation;

namespace Rozmawiator.Data
{
    public static class CallRequestService
    {
        public static List<CallRequest> CallRequests { get; set; } = new List<CallRequest>();

        public static async Task<Conversation> GetConversation(string caller)
        {
            var response = await RestService.CallRequestApi.GetConversation(RestService.CurrentToken, caller);
            if (!response.IsSuccessStatusCode)
            {
                throw new RestErrorException(response.Error);
            }

            var conversation = response.GetModel<ConversationViewModel>();
            if (conversation == null)
            {
                return null;
            }

            return ActiveConversationService.Conversations.FirstOrDefault(c => c.Id == conversation.Id) ??
                   await ConversationService.AddConversation(conversation);
        }
    }
}
