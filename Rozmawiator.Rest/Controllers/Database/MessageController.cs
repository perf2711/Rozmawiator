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
    [RoutePrefix("api/Message")]
    public class MessageController : ApiController
    {
        private readonly RozmawiatorDb _database = new RozmawiatorDb();

        [HttpGet]
        [Route("{id}")]
        public MessageViewModel GetMessage(Guid id)
        {
            var message = _database.Messages.FirstOrDefault(m => m.Id == id);
            if (message == null)
            {
                return null;
            }

            return new MessageViewModel
            {
                Id = message.Id,
                Content = message.Content,
                ConversationId = message.ConversationId,
                Sender = message.Sender.UserName,
                Timestamp = message.Timestamp
            };
        }

        [HttpPost]
        [Route("List")]
        public IEnumerable<MessageViewModel> GetMessages(Dictionary<string, object> filters)
        {
            var user = User.GetUserId();

            return
                new Filter(filters).FilterQuery(
                    _database.Messages.Where(cr => cr.Conversation.ConversationParticipants.Any(cp => cp.UserId == user)))
                    .ToArray()
                    .Select(message => new MessageViewModel
                    {
                        Id = message.Id,
                        Content = message.Content,
                        ConversationId = message.ConversationId,
                        Sender = message.Sender.UserName,
                        Timestamp = message.Timestamp
                    });
        }
    }
}
