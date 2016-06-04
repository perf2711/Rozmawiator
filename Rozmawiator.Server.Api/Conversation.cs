using System;
using System.Collections.Generic;
using System.Linq;
using Rozmawiator.Shared;

namespace Rozmawiator.Server.Api
{
    public class Conversation
    {
        public int Id { get; }
        public Client Owner { get; private set; }
        public Listener Server { get; }
        private readonly List<Client> _participants;
        private readonly List<CallRequest> _callRequests;
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
            Broadcast(new Message(client.Id, 0, Message.MessageType.HelloConversation, client.Nickname));
            ParticipantsUpdate?.Invoke(this);
        }

        public void Disconnect(Client client)
        {
            Server.Debug($"{client.Nickname} disconnected from {Owner.Nickname}'s conversation.");
            _participants.Remove(client);
            if (Owner == client)
            {
                Owner = _participants.FirstOrDefault();
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
                    break;
                case Message.MessageType.Audio:
                    break;
                case Message.MessageType.HelloConversation:
                    break;
                case Message.MessageType.ByeConversation:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Resend(message);
        }

        public void Resend(Message message)
        {
            if (message.Receiver == 0)
            {
                Broadcast(message);
            }
            else
            {
                Server.Send(Server.GetClient(message.Receiver), message);
            }
        }
        

        public void Broadcast(Message message)
        {
            foreach (var client in _participants)
            {
                client.Send(message);
            }
        }
    }
}
