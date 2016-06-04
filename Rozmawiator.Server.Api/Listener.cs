using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Rozmawiator.Shared;

namespace Rozmawiator.Server.Api
{
    public class Listener
    {
        public enum ListenerState
        {
            Idle,
            Listening
        }
    
        private UdpClient _listener;
        private readonly ObservableCollection<Client> _clients;
        private readonly ObservableCollection<Conversation> _conversations;

        public ReadOnlyObservableCollection<Client> Clients { get; }
        public ReadOnlyObservableCollection<Conversation> Conversations { get; }
        

        private short _nextId = 1;
        private int _nextConversationId = 1;

        public bool Timeouts { get; private set; } = false;
        public int TimeoutSpan { get; private set; }
        public int Port { get; private set; }

        public event Action<IPEndPoint, Message> NewMessage;
        public event Action<DateTime, string> DebugMessage;

        public event NotifyCollectionChangedEventHandler ClientListChanged;
        public event Action<Client> ClientConnected;
        public event Action<Client> ClientDisconnected;

        public event NotifyCollectionChangedEventHandler ConversationListChanged;
        public event Action<Conversation> ConversationCreated;
        public event Action<Conversation> ConversationClosed;
        public event Action<Conversation> ConversationUpdate;


        public ListenerState State { get; private set; }

        public Listener()
        {
            _clients = new ObservableCollection<Client>();
            _conversations = new ObservableCollection<Conversation>();
            Clients = new ReadOnlyObservableCollection<Client>(_clients);
            Conversations = new ReadOnlyObservableCollection<Conversation>(_conversations);

            _clients.CollectionChanged += OnClientCollectionChanged;
            _conversations.CollectionChanged += OnConversationCollectionChanged;
        }

        public void Start()
        {
            Port = Configuration.Host.ListenPort;
            TimeoutSpan = Configuration.Client.Timeout;

            _listener = new UdpClient(Port);
            _clients.Clear();
            _conversations.Clear();

            State = ListenerState.Listening;

            Task.Factory.StartNew(ListenLoop);
        }

        public void Stop()
        {
            foreach (var conversation in _conversations.ToArray())
            {
                conversation.Close();
            }

            foreach (var client in _clients.ToArray())
            {
                RemoveClient(client);
            }

            State = ListenerState.Idle;
            _listener.Close();
        }

        private void Send(Client receiver, byte[] message)
        {
            _listener.Send(message, message.Length, receiver.EndPoint);
        }

        public void Send(Client receiver, Message message)
        {
            message.Receiver = receiver.Id;
            Send(receiver, message.GetBytes());
        }

        public void Send(IEnumerable<Client> receivers, Message message)
        {
            foreach (var client in receivers)
            {
                message.Receiver = client.Id;
                Send(client, message);
            }
        }

        public void SendAsClient(Client sender, Client receiver, Message message)
        {
            message.Sender = sender.Id;
            message.Receiver = receiver.Id;
            Send(receiver, message);
        }

        public void SendAsClient(Client sender, IEnumerable<Client> receivers, Message message)
        {
            message.Sender = sender.Id;
            foreach (var client in receivers)
            {
                message.Receiver = client.Id;
                Send(client, message);
            }
        }

        public void Broadcast(Message message)
        {
            message.Sender = 0;
            message.Receiver = 0;
            Send(_clients, message);
        }

        public void Debug(string message)
        {
            DebugMessage?.Invoke(DateTime.Now, message);
        }

        private void ListenLoop()
        {
            while (State == ListenerState.Listening)
            {
                var endpoint = new IPEndPoint(0, 0);
                var msg = _listener.Receive(ref endpoint);

                HandleMessage(endpoint, Message.FromBytes(msg));
            }
        }

        public Client GetClient(IPEndPoint endpoint)
        {
            return _clients.FirstOrDefault(c => Equals(c.EndPoint, endpoint));
        }

        public Client GetClient(short id)
        {
            return _clients.FirstOrDefault(c => c.Id == id);
        }

        public Client GetClient(string nickname)
        {
            return _clients.FirstOrDefault(c => c.Nickname == nickname);
        }

        public Conversation GetInitiatedConversation(Client initator)
        {
            return _conversations.FirstOrDefault(c => c.Owner == initator);
        }

        public Conversation GetConversation(Client member)
        {
            return _conversations.FirstOrDefault(c => c.IsMember(member));
        }

        public Conversation GetConversation(int conversationId)
        {
            return _conversations.FirstOrDefault(c => c.Id == conversationId);
        }

        private void HandleMessage(IPEndPoint endpoint, Message message)
        {
            var sender = GetClient(endpoint);
            message.Sender = sender?.Id ?? 0;
            var conversation = GetConversation(sender);
            switch (message.Type)
            {
                case Message.MessageType.Hello:
                    HandleHello(endpoint, message);
                    break;
                case Message.MessageType.Bye:
                    HandleBye(sender, message);
                    break;
                case Message.MessageType.KeepAlive:
                    HandleKeepAlive(sender, message);
                    break;
                case Message.MessageType.Call:
                    HandleCallTo(sender, message);
                    break;
                case Message.MessageType.CallRequest:
                    break;
                case Message.MessageType.CallResponse:
                    HandleCallResponse(sender, message);
                    break;
                case Message.MessageType.Text:
                case Message.MessageType.Audio:
                case Message.MessageType.HelloConversation:
                case Message.MessageType.ByeConversation:
                    if (sender != null)
                    {
                        conversation?.HandleMessage(message);
                    }
                    break;
                case Message.MessageType.DirectText:
                    HandleDirectText(sender, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            NewMessage?.Invoke(endpoint, message);
        }

        private void HandleDirectText(Client sender, Message message)
        {
            if (sender == null)
            {
                return;
            }

            var targetNickname = message.GetDirectTextNickname();
            var targetClient = GetClient(targetNickname);
            if (targetClient == null)
            {
                Debug($"Undelivered message from {sender.Nickname} to {targetNickname}, target client offline.");
                return;
            }

            SendAsClient(sender, targetClient, message);
        }

        private void HandleHello(IPEndPoint endpoint, Message message)
        {
            var client = GetClient(endpoint);
            if (client != null)
            {
                Debug($"Client with endpoint {endpoint.Address}:{endpoint.Port} already on list ({client.Nickname}).");
                return;
            }

            var nickname = message.GetTextContent();
            var newClient = AddClient(endpoint, nickname);
            Send(newClient, Message.Hello(Configuration.Host.Name));

            Debug($"{nickname} connected ({endpoint.Address}:{endpoint.Port})");
            ClientConnected?.Invoke(newClient);
        }

        private void HandleBye(Client sender, Message message)
        {
            if (sender == null)
            {
                return;
            }
            Debug($"{sender.Nickname} disconnecting ({sender.EndPoint.Address}:{sender.EndPoint.Port})");
            RemoveClient(sender);
        }

        private void HandleKeepAlive(Client sender, Message message)
        {
            //Debug($"{sender.Nickname} is alive ({sender.EndPoint.Address}:{sender.EndPoint.Port})");
            sender?.KeepAlive();
        }

        private void HandleCallTo(Client sender, Message message)
        {
            var calledNickname = message.GetTextContent();
            var calledUser = GetClient(calledNickname);

            Debug($"{sender.Nickname} is trying to call {calledNickname} ({sender.EndPoint.Address}:{sender.EndPoint.Port} -> ?)");

            if (calledUser == null)
            {
                Debug($"{sender.Nickname} was trying to call {calledNickname}, but {calledNickname} is offline.");
                Send(sender, Message.CallResponse(0, 0, Message.CallResponseType.TargetIsOffline));
                return;
            }

            var conversation = GetInitiatedConversation(sender);
            if (conversation == null)
            {
                var currentConversation = GetConversation(sender);
                currentConversation?.Disconnect(sender);
                conversation = NewConversation(sender);
            }
            
            conversation.AddRequest(calledUser);
            conversation.SendRequests();
        }

        private void HandleCallResponse(Client sender, Message message)
        { 
            var callingClient = GetClient(message.Receiver);
            if (callingClient == null)
            {
                Debug($"{sender.Nickname} was trying to respond to call, but caller is offline.");
                Send(sender, Message.CallResponse(0, 0, Message.CallResponseType.TargetIsOffline));
                return;
            }

            var conversation = GetConversation(callingClient);
            if (conversation == null)
            {
                Debug($"{sender.Nickname} was trying to respond to {callingClient.Nickname}'s call, but the conversation no longer exists.");
                Send(sender, Message.CallResponse(0, 0, Message.CallResponseType.ExpiredCall));
                return;
            }

            var request = conversation.GetRequest(callingClient, sender);
            if (request == null)
            {
                Debug($"{sender.Nickname} was trying to respont to {callingClient.Nickname}'s call, but the request no longer exists.");
                Send(sender, Message.CallResponse(0, 0, Message.CallResponseType.ExpiredCall));
                return;
            }

            var response = request.ResolveRequest(message);

            switch (response)
            {
                case Message.CallResponseType.RequestDenied:
                    Debug($"{sender.Nickname} declined {callingClient.Nickname}'s call.");
                    break;
                case Message.CallResponseType.RequestAccepted:
                    var currentConversation = GetConversation(sender);
                    currentConversation?.Disconnect(sender);

                    conversation.Join(sender);
                    Debug($"{sender.Nickname} joined {callingClient.Nickname}'s conversation.");
                    break;
                default:
                    Debug($"Exception: Caller: {callingClient.Nickname}, Target: {sender.Nickname} - unexpected message {response}");
                    break;
            }

            SendAsClient(sender, callingClient, Message.CallResponse(0, 0, response));
            conversation.RemoveRequests();
        }

        private Client AddClient(IPEndPoint endpoint, string nickname)
        {
            var client = new Client(this, _nextId, nickname, endpoint);
            client.Timeout += ClientOnTimeout;
            _clients.Add(client);
            _nextId++;

            return client;
        }

        private void ClientOnTimeout(Client client)
        {
            Debug($"No keepalives - timeout from client {client.Nickname} ({client.Id}) - removing");
            RemoveClient(client);
        }

        private void RemoveClient(Client client)
        {
            GetConversation(client)?.Disconnect(client);
            ClientDisconnected?.Invoke(client);
            _clients.Remove(client);
        }

        private Conversation NewConversation(Client initator)
        {
            var conversation = new Conversation(_nextConversationId, this, initator);
            conversation.CloseConversation += OnCloseConversation;
            conversation.ParticipantsUpdate += OnConversationParticipantsUpdate;
            _conversations.Add(conversation);
            ConversationCreated?.Invoke(conversation);
            return conversation;
        }

        private void OnConversationParticipantsUpdate(Conversation conversation)
        {
            ConversationUpdate?.Invoke(conversation);
        }

        private void OnCloseConversation(Conversation conversation)
        {
            Debug($"Removing conversation {conversation.Id}");
            _conversations.Remove(conversation);
            ConversationClosed?.Invoke(conversation);
        }

        private void OnClientCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            ClientListChanged?.Invoke(this, args);
        }

        private void OnConversationCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            ConversationListChanged?.Invoke(this, args);
        }
    }
}
