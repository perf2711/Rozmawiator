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
    [RoutePrefix("api/Messages")]
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
                SenderId = message.Sender.Id,
                Timestamp = message.Timestamp
            };
        }

        [HttpPost]
        [Route("List")]
        public IEnumerable<MessageViewModel> GetMessages(Dictionary<string, object> filters, int page = 0, int count = 100)
        {
            var user = User.GetUserId();

            return
                new Filter(filters).FilterQuery(
                    _database.Messages.Where(cr => cr.Conversation.Participants.Any(cp => cp.Id == user)))
                    .OrderByDescending(m => m.Timestamp)
                    .Skip(page*count)
                    .Take(count)
                    .ToArray()
                    .Select(message => new MessageViewModel
                    {
                        Id = message.Id,
                        Content = message.Content,
                        ConversationId = message.ConversationId,
                        SenderId = message.Sender.Id,
                        Timestamp = message.Timestamp
                    });
        }

        [HttpPost]
        [Route("List")]
        public IEnumerable<MessageViewModel> GetMessages(Dictionary<string, object> filters, Guid priorTo, int page = 0, int count = 100)
        {
            var user = User.GetUserId();

            var startFromMessage = _database.Messages.FirstOrDefault(m => m.Id == priorTo);
            if (startFromMessage == null)
            {
                return null;
            }

            return
                new Filter(filters).FilterQuery(
                    _database.Messages.Where(cr => cr.Conversation.Participants.Any(cp => cp.Id == user)))
                    .Where(m => m.Timestamp < startFromMessage.Timestamp)
                    .OrderByDescending(m => m.Timestamp)
                    .Skip(page * count)
                    .Take(count)
                    .ToArray()
                    .Select(message => new MessageViewModel
                    {
                        Id = message.Id,
                        Content = message.Content,
                        ConversationId = message.ConversationId,
                        SenderId = message.Sender.Id,
                        Timestamp = message.Timestamp
                    });
        }
    }
}
