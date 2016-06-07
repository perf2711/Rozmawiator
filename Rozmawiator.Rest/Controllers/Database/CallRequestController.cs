using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security;
using Rozmawiator.Database;
using Rozmawiator.Database.Entities;
using Rozmawiator.Database.Identity;
using Rozmawiator.Database.ViewModels;

namespace Rozmawiator.Rest.Controllers.Database
{
    [Authorize]
    [RoutePrefix("api/CallRequests")]
    public class CallRequestController : ApiController
    {
        private readonly RozmawiatorDb _database = new RozmawiatorDb();

        [HttpGet]
        [Route("{id}")]
        public CallRequestViewModel GetCallRequest(Guid id)
        {
            var callRequest = _database.CallRequests.FirstOrDefault(cr => cr.Id == id);
            if (callRequest == null)
            {
                return null;
            }

            return new CallRequestViewModel
            {
                Id = callRequest.Id,
                Timestamp = callRequest.Timestamp,
                ConversationId = callRequest.ConversationId
            };
        }
        
        [HttpPost]
        [Route("List")]
        public IEnumerable<CallRequestViewModel> GetCallRequests(Dictionary<string, object> filters)
        {
            var user = User.GetUserId();

            return
                new Filter(filters).FilterQuery(
                    _database.CallRequests.Where(c => c.Conversation.Participants.Any(p => p.Id == user)))
                        .ToArray()
                        .Select(callRequest => new CallRequestViewModel
                        {
                            Id = callRequest.Id,
                            Timestamp = callRequest.Timestamp,
                            ConversationId = callRequest.ConversationId
                        });
        }

        /*
        [HttpGet]
        [Route("Conversation/{caller}")]
        public ConversationViewModel GetCurrentCallRequestConversation(Guid conversationId)
        {
            var userId = User.GetUserId();
            var user = _database.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return null;
            }

            var callRequest = _database.CallRequests.FirstOrDefault(c => );
            if (callRequest == null)
            {
                return null;
            }

            var conversation =
                _database.Conversations.FirstOrDefault(
                    c =>
                        c.Type == ConversationType.Active &&
                        c.ConversationParticipants.FirstOrDefault(p => p.User.UserName == caller).IsActive);

            if (conversation == null)
            {
                return null;
            }

            return new ConversationViewModel
            {
                Id = conversation.Id,
                Type = ConversationType.Active,
                Creator = conversation.Creator.UserName,
                Owner = conversation.Owner.UserName,
                Participants = conversation.ConversationParticipants.Where(p => p.IsActive).Select(p => p.User.UserName).ToArray()
            };
        }
        */
    }
}
