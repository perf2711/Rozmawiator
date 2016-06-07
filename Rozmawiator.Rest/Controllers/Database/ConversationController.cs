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
                Participants = conversation.Participants.Select(cp => cp.Id).ToArray()
            };
        }

        [HttpPost]
        [Route("List")]
        public IEnumerable<ConversationViewModel> GetConversations(Dictionary<string, object> filters)
        {
            var user = User.GetUserId();

            return
                new Filter(filters).FilterQuery(
                    _database.Conversations.Where(cr => cr.Participants.Any(cp => cp.Id == user)))
                    .ToArray()
                    .Select(conversation => new ConversationViewModel
                    {
                        Id = conversation.Id,
                        Participants = conversation.Participants.Select(cp => cp.Id).ToArray()
                    });
        }
    }
}
