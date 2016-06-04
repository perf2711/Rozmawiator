using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rozmawiator.Database;
using Rozmawiator.Database.Identity;
using Rozmawiator.Database.ViewModels;

namespace Rozmawiator.Rest.Controllers.Database
{
    [Authorize]
    [RoutePrefix("api/Conversations")]
    public class ConversationController : ApiController
    {
        private readonly RozmawiatorDb _database = new RozmawiatorDb();

        [HttpGet]
        [Route("{id}")]
        public ConversationViewModel GetConversation(Guid id)
        {
            var conversation = _database.Conversations.FirstOrDefault(s => s.Id == id);
            if (conversation == null)
            {
                return null;
            }

            return new ConversationViewModel
            {
                Id = conversation.Id,
                Type = conversation.Type,
                Creator = conversation.Creator.UserName,
                Owner = conversation.Owner.UserName
            };
        }

        [HttpPost]
        [Route("List")]
        public IEnumerable<ConversationViewModel> GetConversations(Dictionary<string, object> filters)
        {
            var user = User.GetUserId();

            return
                new Filter(filters).FilterQuery(
                    _database.Conversations.Where(cr => cr.ConversationParticipants.Any(cp => cp.UserId == user)))
                    .ToArray()
                    .Select(conversation => new ConversationViewModel
                    {
                        Id = conversation.Id,
                        Type = conversation.Type,
                        Creator = conversation.Creator.UserName,
                        Owner = conversation.Owner.UserName
                    });
        }
    }
}
