using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rozmawiator.Database;
using Rozmawiator.Database.Entities;
using Message = Rozmawiator.Shared.Message;

namespace Rozmawiator.Server.Api
{
    public class Conversation
    {
        public int Id { get; }
        public Client Owner { get; private set; }
        public Listener Server { get; }
        private readonly List<Client> _participants;
        private readonly List<CallRequest> _callRequests;
        private readonly Guid _conversationId = Guid.Empty;
        public IEnumerable<Client> Participants => _participants.AsReadOnly();
        public IEnumerable<CallRequest> CallRequests => _callRequests.AsReadOnly();

        public event Action<Conversation> ParticipantsUpdate;

        public event Action<Conversation> CloseConversation;
        
        public Conversation(int id, Listener server, Client owner)
        {
            Id = id;
            Owner = owner;
            Server = server;
            _participants = new List<Client>
            {
                owner
            };
            _callRequests = new List<CallRequest>();

            using (var database = new RozmawiatorDb())
            {
                var ownerUser = database.Users.FirstOrDefault(u => u.UserName == owner.Nickname);
                if (ownerUser == null)
                {
                    return;
                }

                var conversation = new Database.Entities.Conversation
                {
                    Type = ConversationType.Active,
                    Owner = ownerUser,
                    Creator = ownerUser,
                    ConversationParticipants = new[]
                    {
                        new ConversationParticipant
                        {
                            User = ownerUser
                        }
                    }
                };
                _conversationId = conversation.Id;
                database.Conversations.Add(conversation);
                database.SaveChanges();
            }
        }

        public bool IsMember(Client client)
        {
            return Owner == client || _participants.Contains(client);
        }

        public void AddRequest(Client target)
        {
            _callRequests.Add(new CallRequest(Server, Owner, target));
        }

        public void SendRequests()
        {
            foreach (var request in _callRequests)
            {
                if (request.State == CallRequest.RequestState.Untouched)
                {
                    request.SendRequest();
                }
            }
        }

        public void RemoveRequests()
        {
            var requestsToRemove = _callRequests.Where(request => request.State == CallRequest.RequestState.Resolved).ToArray();

            foreach (var requestToRemove in requestsToRemove)
            {
                _callRequests.Remove(requestToRemove);
            }

            TryClose();
        }

        public CallRequest GetRequest(Client caller, Client target)
        {
            return _callRequests.FirstOrDefault(c => c.RequestingClient == caller && c.TargetClient == target);
        }

        public void Join(Client client)
        {
            _participants.Add(client);

            using (var database = new RozmawiatorDb())
            {
                var user =
                    database.Users.Select(u => new {u.Id, u.UserName})
                        .FirstOrDefault(u => u.UserName == client.Nickname);
                if (user != null)
                {
                    var conversation = database.Conversations.First(c => c.Id == _conversationId);
                    var participant = conversation.ConversationParticipants.FirstOrDefault(p => p.UserId == user.Id);
                    if (participant != null)
                    {
                        participant.IsActive = true;
                    }
                    else
                    {
                        participant = new ConversationParticipant
                        {
                            Conversation = conversation,
                            UserId = user.Id
                        };
                        conversation.ConversationParticipants.Add(participant);
                    }
                }

                database.SaveChanges();
            }

            Broadcast(new Message(client.Id, 0, Message.MessageType.HelloConversation, client.Nickname));
            ParticipantsUpdate?.Invoke(this);
        }

        public void Disconnect(Client client)
        {
            Server.Debug($"{client.Nickname} disconnected from {Owner.Nickname}'s conversation.");
            _participants.Remove(client);
            using (var database = new RozmawiatorDb())
            {
                var conversation = database.Conversations.First(c => c.Id == _conversationId);

                if (Owner == client)
                {
                    Owner = _participants.FirstOrDefault();
                    if (Owner != null)
                    {
                        var newOwnerUser = database.Users.Select(u => new { u.Id, u.UserName })
                            .FirstOrDefault(u => u.UserName == Owner.Nickname);
                        conversation.OwnerId = newOwnerUser?.Id ?? Guid.Empty;
                    }
                }

                var user =
                    database.Users.Select(u => new { u.Id, u.UserName })
                        .FirstOrDefault(u => u.UserName == client.Nickname);

                if (user != null)
                {
                    var conversationParticipant =
                        conversation.ConversationParticipants.FirstOrDefault(p => p.UserId == user.Id);

                    if (conversationParticipant != null)
                    {
                        conversationParticipant.IsActive = false;
                    }
                }

                database.SaveChanges();
            }

            //Broadcast(new Message(client.Id, 0, Message.MessageType.ByeConversation, client.Nickname));
            Server.SendAsClient(client, _participants, new Message(Message.MessageType.ByeConversation, client.Nickname));
            ParticipantsUpdate?.Invoke(this);
            TryClose();
        }

        private void TryClose()
        {
            // If there is only one participant (owner), and all requests were aborted, close the conversation
            if (_participants.Count < 2 && !_callRequests.Any())
            {
                Broadcast(new Message(Message.MessageType.CloseConversation, "NoMembersLeft"));
                CloseConversation?.Invoke(this);
            }
        }

        public void Close()
        {
            Broadcast(new Message(Message.MessageType.CloseConversation, "ClosedByServer"));
            CloseConversation?.Invoke(this);
        }

        public void HandleMessage(Message message)
        {
            switch (message.Type)
            {
                case Message.MessageType.Hello:
                case Message.MessageType.Bye:
                case Message.MessageType.KeepAlive:
                case Message.MessageType.Call:
                case Message.MessageType.CallRequest:
                case Message.MessageType.CallResponse:
                case Message.MessageType.CloseConversation:
                case Message.MessageType.DirectText:
                    return;
                case Message.MessageType.Text:
                    SaveMessage(message);
                    break;
                case Message.MessageType.Audio:
                case Message.MessageType.HelloConversation:
                case Message.MessageType.ByeConversation:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Resend(message);
        }

        public void Resend(Message message)
        {
            Broadcast(message);
            return;
            /*
            if (message.Receiver == 0)
            {
                Broadcast(message);
            }
            else
            {
                Server.Send(Server.GetClient(message.Receiver), message);
            }
            */
        }

        public void Broadcast(Message message)
        {
            foreach (var client in _participants)
            {
                client.Send(message);
            }
        }

        private void SaveMessage(Message message)
        {
            var sender = _participants.FirstOrDefault(p => p.Id == message.Sender);
            using (var database = new RozmawiatorDb())
            {
                var senderUser = database.Users.FirstOrDefault(u => u.UserName == sender.Nickname);
                if (senderUser == null)
                {
                    return;
                }

                var conversation = database.Conversations.First(c => c.Id == _conversationId);

                var msg = new Database.Entities.Message
                {
                    Content = message.GetTextContent(),
                    Conversation = conversation,
                    Sender = senderUser,
                    Timestamp = DateTime.Now
                };
                database.Messages.Add(msg);
                database.SaveChanges();
            }
        }
    }
}
