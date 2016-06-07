using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;

namespace Rozmawiator.ClientApi
{
    public class Call
    {
        public Guid Id { get; }
        public Conversation Conversation { get; }

        public ReadOnlyObservableCollection<Guid> Participants { get; }
        private readonly ObservableCollection<Guid> _paricipants;

        public event Action<Call, Guid> NewParticipant;
        public event Action<Call, Guid> ParticipantDeclined;
        public event Action<Call, Guid> ParticipantLeft;

        public event Action<Call, CallMessage> NewAudio;

        public Call(Guid id, Conversation conversation)
        {
            Id = id;
            Conversation = conversation;

            _paricipants = new ObservableCollection<Guid>();
            Participants = new ReadOnlyObservableCollection<Guid>(_paricipants);
        }

        public bool IsMember(Guid guid)
        {
            return _paricipants.Contains(guid);
        }

        public void AddUser(Guid id)
        {
            _paricipants.Add(id);
        }

        public void Disconnect()
        {
            Send(CallMessage.Create(Conversation.Client.Id, Id).Bye("Disconnect"));
        }

        public void HandleCallMessage(CallMessage message)
        {
            switch (message.Type)
            {
                case CallMessageType.NewUser:
                    HandleNewUser(message);
                    break;
                case CallMessageType.UserDeclined:
                    HandleUserDeclined(message);
                    break;
                case CallMessageType.Bye:
                    HandleBye(message);
                    break;
                case CallMessageType.UserLeft:
                    HandleUserLeft(message);
                    break;
                case CallMessageType.Audio:
                    HandleAudio(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Send(CallMessage message)
        {
            Conversation.Client.Send(message);
        }

        private void HandleNewUser(CallMessage message)
        {
            var id = message.GetGuidContent();
            _paricipants.Add(id);
            NewParticipant?.Invoke(this, id);
        }

        private void HandleUserDeclined(CallMessage message)
        {
            ParticipantDeclined?.Invoke(this, message.GetGuidContent());
        }

        private void HandleBye(CallMessage message)
        {
            Conversation.DisconnectCall();
        }

        private void HandleUserLeft(CallMessage message)
        {
            var id = message.GetGuidContent();
            _paricipants.Remove(id);
            ParticipantLeft?.Invoke(this, id);
        }

        private void HandleAudio(CallMessage message)
        {
            NewAudio?.Invoke(this, message);
        }
    }
}
