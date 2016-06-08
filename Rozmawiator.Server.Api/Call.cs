using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Database;

namespace Rozmawiator.Server.Api
{
    public class Call
    {
        public Guid Id { get; }
        public Conversation Conversation { get; }
        public Listener Server { get; }
        public IEnumerable<Client> Participants => _participants.AsReadOnly();

        public event Action<Call> ParticipantsUpdate;
        public event Action<Call> CallEnded;

        private readonly List<Client> _participants;

        public Call(Conversation conversation, Listener server)
        {
            Id = Guid.NewGuid();
            Conversation = conversation;
            Server = server;
            _participants = new List<Client>();

            using (var database = new RozmawiatorDb())
            {
                var call = new Database.Entities.Call
                {
                    Id = Id,
                    ConversationId = conversation.Id,
                    Timestamp = DateTime.Now
                };
                database.Calls.Add(call);
                database.SaveChanges();
            }
        }

        public bool IsMember(Client client)
        {
            return _participants.Contains(client);
        }

        public void Join(Client client)
        {
            using (var database = new RozmawiatorDb())
            {
                var call = database.Calls.Find(Id);
                var user = database.Users.Find(client.Id);
                if (call == null || user == null)
                {
                    return;
                }

                call.Participants.Add(user);
                database.SaveChanges();
            }

            _participants.Add(client);

            Broadcast(CallMessage.Create(Server.ServerId, Id).NewUser(client.User.Id));
            ParticipantsUpdate?.Invoke(this);
        }

        public void Disconnect(Client client, bool tryClose = true)
        {
            Server.Debug($"{client?.User?.Id} left from call {Id}.");
            _participants.Remove(client);
            using (var database = new RozmawiatorDb())
            {
                var call = database.Calls.Find(Id);
                var user = database.Users.Find(client.User.Id);

                if (call != null && user != null)
                {
                    call.Participants.Remove(user);
                    database.SaveChanges();
                }
            }

            Broadcast(CallMessage.Create(Server.ServerId, Id).UserLeft(client.User.Id));
            ParticipantsUpdate?.Invoke(this);

            if (tryClose)
            {
                TryClose();
            }
        }

        public void HandleMessage(CallMessage message)
        {
            switch (message.Type)
            {
                case CallMessageType.NewUser:
                    break;
                case CallMessageType.Bye:
                    HandleBye(message);
                    return;
                case CallMessageType.UserLeft:
                    break;
                case CallMessageType.Audio:
                    Broadcast(message, true);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Broadcast(message);
        }

        private void HandleBye(CallMessage message)
        {
            Disconnect(Server.GetClient(message.SenderId));
            Broadcast(CallMessage.Create(Server.ServerId, Id).UserLeft(message.SenderId), message.SenderId);
        }

        public void Broadcast(Message message, params Client[] skipClients)
        {
            var participants = skipClients.Any()
                ? _participants.Where(p => !skipClients.Contains(p))
                : _participants;

            foreach (var client in participants)
            {
                client.Send(message);
            }
        }

        public void Broadcast(Message message, params Guid[] skipClients)
        {
            var participants = skipClients.Any()
                ? _participants.Where(p => !skipClients.Contains(p.Id))
                : _participants;

            foreach (var client in participants)
            {
                client.Send(message);
            }
        }


        public void Broadcast(Message message, bool skipSender = false)
        {
            var sender = Server.GetClient(message.SenderId);
            foreach (var client in _participants.Where(p => !skipSender || p != sender))
            {
                client.Send(message);
            }
        }

        public void TryClose()
        {
            if (_participants.Count > 1)
            {
                return;
            }

            if (_participants.Count == 1)
            {
                Broadcast(CallMessage.Create(Server.ServerId, Id).Bye("NoUsersLeft"));
                Disconnect(_participants.First(), false);
            }

            Conversation.Broadcast(ConversationMessage.Create(Server.ServerId, Conversation.Id).RevokeCallRequest(Id));
            CallEnded?.Invoke(this);
        }
    }
}
