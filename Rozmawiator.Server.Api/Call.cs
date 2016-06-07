using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Database;

namespace Rozmawiator.Server.Api
{
    public class Call
    {
        public Guid Id { get; }
        public Listener Server { get; }
        public IEnumerable<Client> Participants => _participants.AsReadOnly();

        public event Action<Call> ParticipantsUpdate;
        public event Action<Call> CallEnded;

        private readonly List<Client> _participants;

        public Call(Listener server)
        {
            Server = server;
            _participants = new List<Client>();

            using (var database = new RozmawiatorDb())
            {
                var call = new Database.Entities.Call
                {
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
            _participants.Add(client);

            using (var database = new RozmawiatorDb())
            {
                var call = database.Calls.First();
                call.Participants.Add(client.User);
                database.SaveChanges();
            }

            Broadcast(CallMessage.Create(Server.ServerId, Id).NewUser(client.User.Id));
            ParticipantsUpdate?.Invoke(this);
        }

        public void Disconnect(Client client)
        {
            Server.Debug($"{client.User.Id} left from call {Id}.");
            _participants.Remove(client);
            using (var database = new RozmawiatorDb())
            {
                var call = database.Calls.Find(Id);
                var user = database.Users.Find(client.User.Id);
                call.Participants.Remove(user);
                database.SaveChanges();
            }

            Broadcast(CallMessage.Create(Server.ServerId, Id).UserLeft(client.User.Id));
            ParticipantsUpdate?.Invoke(this);

            TryClose();
        }

        public void HandleMessage(CallMessage message)
        {
            switch (message.Type)
            {
                case CallMessageType.NewUser:
                    break;
                case CallMessageType.Bye:
                    Disconnect(Server.GetClient(message.SenderId));
                    break;
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
                Disconnect(_participants.First());
                Broadcast(CallMessage.Create(Server.ServerId, Id).Bye("NoUsersLeft"));
            }
            CallEnded?.Invoke(this);
        }
    }
}
