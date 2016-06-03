using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Shared;

namespace Rozmawiator.ClientApi
{
    public class Conversation
    {
        public Client Client { get; }
        public ReadOnlyObservableCollection<RemoteClient> Participants { get; }

        public event Action<Conversation, RemoteClient> ParticipantConnected;
        public event Action<Conversation, RemoteClient> ParticipantDisconnected;
        public event Action<Conversation, Message> ConversationClosed;
        public event Action<Conversation, RemoteClient, Message> NewTextMessage;
        public event Action<Conversation, RemoteClient, Message> NewAudioMessage;

        private readonly ObservableCollection<RemoteClient> _participants;

        public Conversation(Client client)
        {
            Client = client;
            _participants = new ObservableCollection<RemoteClient>();
            Participants = new ReadOnlyObservableCollection<RemoteClient>(_participants);
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
                    return;
                case Message.MessageType.Text:
                    HandleTextMessage(message);
                    break;
                case Message.MessageType.Audio:
                    HandleAudioMessage(message);
                    break;
                case Message.MessageType.HelloConversation:
                    HandleHelloConversation(message);
                    break;
                case Message.MessageType.ByeConversation:
                    HandleByeConversation(message);
                    break;
                case Message.MessageType.CloseConversation:
                    HandleCloseConversation(message);
                    break;
            }
        }

        public RemoteClient GetRemoteClient(short id)
        {
            return Participants.FirstOrDefault(p => p.Id == id);
        }

        public RemoteClient GetSender(Message message)
        {
            return Participants.FirstOrDefault(p => p.Id == message.Sender);
        }

        private void HandleTextMessage(Message message)
        {
            NewTextMessage?.Invoke(this, GetSender(message), message);
        }

        private void HandleAudioMessage(Message message)
        {
            NewAudioMessage?.Invoke(this, GetSender(message), message);
        }

        private void HandleHelloConversation(Message message)
        {
            var remoteClient = new RemoteClient(message.Sender, message.GetTextContent());
            _participants.Add(remoteClient);
            ParticipantConnected?.Invoke(this, remoteClient);
        }

        private void HandleByeConversation(Message message)
        {
            var remoteClient = Participants.FirstOrDefault(p => p.Id == message.Sender);
            if (remoteClient == null)
            {
                return;
            }
            _participants.Remove(remoteClient);
            ParticipantDisconnected?.Invoke(this, remoteClient);
        }

        private void HandleCloseConversation(Message message)
        {
            SendToAll(new Message(Message.MessageType.ByeConversation, "Disconnect"));
            ConversationClosed?.Invoke(this, message);
        }

        public void Send(Message message)
        {
            Client.Send(message);
        }

        public void SendToAll(Message message)
        {
            message.Receiver = 0;
            Send(message);
        }

        public void SendTo(string nickname, Message message)
        {
            var id = _participants.FirstOrDefault(p => p.Nickname == nickname)?.Id;
            if (id == null)
            {
                return;
            }
            message.Receiver = id.Value;

            Send(message);
        }

        public void SendTo(short id, Message message)
        {
            message.Receiver = id;
            Send(message);
        }
    }
}
