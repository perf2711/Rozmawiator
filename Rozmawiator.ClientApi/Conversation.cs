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
        public IReadOnlyList<ConversationMessage> TextMessages => _textMessages.AsReadOnly();

        public event Action<Conversation, Guid> ParticipantConnected;
        public event Action<Conversation, Guid> ParticipantDisconnected;

        public event Action<Conversation, CallRequest> NewCallRequest;
        public event Action<Conversation, Guid> CallRequestRevoked;
        public event Action<Conversation, ConversationMessage> NewTextMessage;

        public event Action<Conversation, Call> NewCall;

        private readonly ObservableCollection<Guid> _participants;
        private readonly List<ConversationMessage> _textMessages;

        public Conversation(Guid id, Client client, params Guid[] participants)
        {
            Id = id;
            Client = client;

            _textMessages = new List<ConversationMessage>();

            _participants = new ObservableCollection<Guid>(participants);
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
                    HandleCallRequest(message);
                    break;
                case ConversationMessageType.RevokeRequest:
                    HandleCallRequestRevoked(message);
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
            _participants.Add(message.GetGuidContent());
            ParticipantConnected?.Invoke(this, message.GetGuidContent());
        }

        private void HandleUserLeft(ConversationMessage message)
        {
            _participants.Remove(message.GetGuidContent());
            ParticipantDisconnected?.Invoke(this, message.GetGuidContent());
        }

        private void HandleTextMessage(ConversationMessage message)
        {
            _textMessages.Add(message);
            NewTextMessage?.Invoke(this, message);
        }

        private void HandleCallRequest(ConversationMessage message)
        {
            var callRequest = new CallRequest(this, message.GetGuidContent());
            NewCallRequest?.Invoke(this, callRequest);
        }

        private void HandleCallRequestRevoked(ConversationMessage message)
        {
            CallRequestRevoked?.Invoke(this, message.GetGuidContent());
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

        private void OnCallCreated(ExpectedMessage expectedMessage, IMessage message)
        {
            var callId = ((ServerMessage)message).GetGuidContent();
            Call = new Call(callId, this);
            NewCall?.Invoke(this, Call);
        }

        public void RespondToRequest(CallRequest request)
        {
            if (request.Response == null)
            {
                return;
            }

            switch (request.Response.Value)
            {
                case CallResponseType.Accepted:
                    Call = new Call(request.CallId, this);
                    NewCall?.Invoke(this, Call);
                    break;
                case CallResponseType.Denied:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
