using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Communication.Server;
using Rozmawiator.Request;

namespace Rozmawiator.ClientApi
{
    public class Conversation
    {
        public Guid Id { get; }
        public Client Client { get; }
        public Call Call { get; private set; }
        public ReadOnlyObservableCollection<Guid> Participants { get; }
        public IReadOnlyList<Message> TextMessages => _textMessages.AsReadOnly();

        public event Action<Conversation, Guid> ParticipantConnected;
        public event Action<Conversation, Guid> ParticipantDisconnected;

        public event Action<Conversation, CallRequest> NewCallRequest;
        public event Action<Conversation, ConversationMessage> NewTextMessage;

        private readonly ObservableCollection<Guid> _participants;
        private readonly List<Message> _textMessages;

        public Conversation(Guid id, Client client)
        {
            Id = id;
            Client = client;

            _textMessages = new List<Message>();

            _participants = new ObservableCollection<Guid>();
            Participants = new ReadOnlyObservableCollection<Guid>(_participants);
        }

        public void HandleConversationMessage(ConversationMessage message)
        {
            switch (message.Type)
            {
                case ConversationMessageType.NewUser:
                    HandleNewUser(message);
                    break;
                case ConversationMessageType.UserLeft:
                    HandleUserLeft(message);
                    break;
                case ConversationMessageType.Text:
                    HandleTextMessage(message);
                    break;
                case ConversationMessageType.CallRequest:
                    break;
                case ConversationMessageType.AddUser:
                case ConversationMessageType.Bye:
                case ConversationMessageType.CreateCall:
                case ConversationMessageType.CallResponse:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleNewUser(ConversationMessage message)
        {
            _participants.Add(message.SenderId);
            ParticipantConnected?.Invoke(this, message.SenderId);
        }

        private void HandleUserLeft(ConversationMessage message)
        {
            _participants.Remove(message.SenderId);
            ParticipantDisconnected?.Invoke(this, message.SenderId);
        }

        private void HandleTextMessage(ConversationMessage message)
        {
            _textMessages.Add(message);
            NewTextMessage?.Invoke(this, message);
        }

        public void AddUser(Guid id)
        {
            Client.Send(ConversationMessage.Create(Client.Id, Id).AddUser(id));
        }

        public void CreateCall()
        {
            Client.ExpectedMessages.Add(new ExpectedMessage(ServerMessageType.Ok, OnCallCreated));
            Client.Send(ConversationMessage.Create(Client.Id, Id).CreateCall());
        }

        private void OnCallCreated(ExpectedMessage expectedMessage, Message message)
        {
            var callId = message.GetGuidContent();
            Call = new Call(callId, this);
        }

        public void RespondToRequest(CallRequest request)
        {
            if (request.Response == null)
            {
                return;
            }

            Client.Send(ConversationMessage.Create(Client.Id, Id).CallResponse(request.Response.Value));
        }

        public void DisconnectCall()
        {
            Call?.Disconnect();
            Call = null;
        }

        public void Send(ConversationMessage message)
        {
            if (message.Type == ConversationMessageType.Text)
            {
                _textMessages.Add(message);
            }
            Client.Send(message);
        }

        public void Disconnect()
        {
            Client.Send(ConversationMessage.Create(Client.Id, Id).Bye("Disconnect"));
        }
    }
}
