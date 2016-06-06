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
        public ReadOnlyObservableCollection<string> CalledClients { get; }
        public IReadOnlyList<Message> TextMessages => _textMessages.AsReadOnly();

        public event Action<Conversation, RemoteClient> ParticipantConnected;
        public event Action<Conversation, RemoteClient> ParticipantDisconnected;
        public event Action<Conversation, Message> ConversationClosed;
        public event Action<Conversation, RemoteClient, Message> NewTextMessage;
        public event Action<Conversation, RemoteClient, Message> NewAudioMessage;

        private readonly ObservableCollection<RemoteClient> _participants;
        private readonly ObservableCollection<string> _calledClients;
        private readonly List<Message> _textMessages;

        public Conversation(Client client)
        {
            Client = client;
            _participants = new ObservableCollection<RemoteClient>();
            _calledClients = new ObservableCollection<string>();
            _textMessages = new List<Message>();
            Participants = new ReadOnlyObservableCollection<RemoteClient>(_participants);
            CalledClients = new ReadOnlyObservableCollection<string>(_calledClients);
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
            return _participants.FirstOrDefault(p => p.Id == id);
        }

        public RemoteClient GetSender(Message message)
        {
            return _participants.FirstOrDefault(p => p.Id == message.Sender);
        }

        private void HandleTextMessage(Message message)
        {
            _textMessages.Add(message);
            NewTextMessage?.Invoke(this, GetSender(message), message);
        }

        private void HandleAudioMessage(Message message)
        {
            NewAudioMessage?.Invoke(this, GetSender(message), message);
        }

        private void HandleHelloConversation(Message message)
        {
            var remoteClient = new RemoteClient(message.Sender, message.GetTextContent());
            _calledClients.Remove(remoteClient.Nickname);
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

        public void AddUser(string nickname)
        {
            Client.Send(Message.CreateNew.Call(nickname));
            _calledClients.Add(nickname);
        }

        public void Send(Message message)
        {
            if (message.Type == Message.MessageType.Text)
            {
                _textMessages.Add(message);
            }
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

        public void Disconnect()
        {
            Client.DisconnectFromConversation("Disconnect");
        }
    }
}
