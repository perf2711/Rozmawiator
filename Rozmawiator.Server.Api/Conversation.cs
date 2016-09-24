using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Communication.Server;
using Rozmawiator.Database;
using Rozmawiator.Database.Entities;
using Message = Rozmawiator.Communication.Message;

namespace Rozmawiator.Server.Api
{
    public class Conversation
    {
        public Guid Id { get; }
        public Listener Server { get; }
        public IEnumerable<Client> Participants => _participants.AsReadOnly();
        public Call Call { get; set; }

        public event Action<Conversation> ParticipantsUpdate;

        private readonly List<Client> _participants;

        public Conversation(Guid id, Listener server)
        {
            Id = id;
            Server = server;
            _participants = new List<Client>();

            using (var database = new RozmawiatorDb())
            {
                var conversation = database.Conversations.FirstOrDefault(c => c.Id == Id);
                if (conversation != null)
                {
                    var participants =
                        conversation.Participants.ToArray().Select(p => Server.GetClient(p.Id)).Where(c => c != null);
                    _participants.AddRange(participants);
                    return;
                }

                conversation = new Database.Entities.Conversation {Id = id};
                database.Conversations.Add(conversation);
                database.SaveChanges();
            }
        }

        public bool IsMember(Client client)
        {
            return _participants.Contains(client);
        }

        public void JoinOffline(Guid clientId)
        {
            using (var database = new RozmawiatorDb())
            {
                var user = database.Users.Find(clientId);
                if (user == null)
                {
                    return;
                }
                var conversation = database.Conversations.Find(Id);

                conversation.Participants.Add(user);
                database.SaveChanges();
            }

            Broadcast(ConversationMessage.Create(Server.ServerId, Id).NewUser(clientId));
        }

        public void Join(Client client)
        {
            _participants.Add(client);

            using (var database = new RozmawiatorDb())
            {
                var conversation = database.Conversations.Find(Id);
                var user = database.Users.Find(client.User.Id);
                conversation.Participants.Add(user);
                database.SaveChanges();
            }

            Broadcast(ConversationMessage.Create(Server.ServerId, Id).NewUser(client.User.Id));
            ParticipantsUpdate?.Invoke(this);
        }

        public void JoinMember(Client client)
        {
            _participants.Add(client);
            Broadcast(ConversationMessage.Create(Server.ServerId, Id).NewUser(client.User.Id));
            ParticipantsUpdate?.Invoke(this);
        }

        public void Disconnect(Client client)
        {
            Call?.Disconnect(client);
            _participants.Remove(client);
            ParticipantsUpdate?.Invoke(this);
        }

        public void HandleMessage(ConversationMessage message)
        {
            switch (message.Type)
            {
                case ConversationMessageType.AddUser:
                    HandleAddUser(message);
                    return;
                case ConversationMessageType.Bye:
                    Disconnect(Server.GetClient(message.SenderId));
                    break;
                case ConversationMessageType.NewUser:
                    break;
                case ConversationMessageType.UserLeft:
                    break;
                case ConversationMessageType.Text:
                    SaveMessage(message);
                    break;
                case ConversationMessageType.CreateCall:
                    CreateCall(message);
                    break;
                case ConversationMessageType.CallRequest:
                    return;
                case ConversationMessageType.CallResponse:
                    HandleCallResponse(message);
                    break;
                case ConversationMessageType.RevokeRequest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            } 

            Broadcast(message);
        }

        public void HandleMessage(CallMessage message)
        {
            Call?.HandleMessage(message);
        }

        public void CreateCall(ConversationMessage message)
        {
            if (Call?.Participants?.Any() == true)
            {
                return;
            }

            Call = new Call(this, Server);
            Call.Join(Server.GetClient(message.SenderId));
            Server.Send(Server.GetClient(message.SenderId), ServerMessage.Create(Id).Ok(Call.Id.ToByteArray()));
            Broadcast(ConversationMessage.Create(Server.ServerId, Id).CallRequest(Call.Id), message.SenderId);
        }

        private void HandleAddUser(ConversationMessage message)
        {
            var id = message.GetGuidContent();
            var client = Server.GetClient(id);
            if (client == null)
            {
                JoinOffline(id);
            }
            else
            {
                Join(client);
            }
        }

        private void HandleCallResponse(ConversationMessage message)
        {
            if (Call == null)
            {
                return;
            }

            var response = (CallResponseType) message.GetByteContent();
            switch (response)
            {
                case CallResponseType.Accepted:
                    Call.Join(Server.GetClient(message.SenderId));
                    break;
                case CallResponseType.Denied:
                    Call.Broadcast(CallMessage.Create(Server.ServerId, Id).UserDeclined(message.SenderId));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            foreach (var client in _participants.Where(p => !skipSender || p.Id != sender?.Id))
            {
                client.Send(message);
            }
        }

        private void SaveMessage(ConversationMessage message)
        {
            using (var database = new RozmawiatorDb())
            {
                var conversation = database.Conversations.First(c => c.Id == Id);

                var msg = new Database.Entities.Message
                {
                    Content = message.GetStringContent(), Conversation = conversation, SenderId = message.SenderId, Timestamp = DateTime.Now
                };
                database.Messages.Add(msg);
                database.SaveChanges();
            }
        }
    }
}
