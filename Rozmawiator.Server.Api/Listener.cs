using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Communication.Server;
using Rozmawiator.Database;
using Rozmawiator.Database.Entities;
using Message = Rozmawiator.Communication.Message;

namespace Rozmawiator.Server.Api
{
    public class Listener
    {
        public enum ListenerState
        {
            Idle,
            Listening
        }

        public Guid ServerId { get; } = Guid.Parse("20238f7a-4dcb-405b-ac8d-f72ae7591bcb");
    
        private UdpClient _listener;
        private readonly ObservableCollection<Client> _clients;
        private readonly ObservableCollection<Conversation> _conversations;

        public ReadOnlyObservableCollection<Client> Clients { get; }
        public ReadOnlyObservableCollection<Conversation> Conversations { get; }

        public bool Timeouts { get; private set; } = false;
        public int TimeoutSpan { get; private set; }
        public int Port { get; private set; }

        public event Action<IPEndPoint, IMessage> NewMessage;
        public event Action<DateTime, string> DebugMessage;
        public event Action<IPEndPoint, IMessage> MessageSent;

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
            using (var database = new RozmawiatorDb())
            {
                var server = database.Servers.Find(ServerId);
                if (server == null)
                {
                    server = new Database.Entities.Server
                    {
                        Id = ServerId,
                    };
                    database.Servers.Add(server);
                }
                server.IpAddress = "188.226.229.215";
                server.Port = Configuration.Host.ListenPort;
                server.State = ServerState.Online;
                database.SaveChanges();
            }

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
            using (var database = new RozmawiatorDb())
            {
                var server = database.Servers.Find(ServerId);
                if (server != null)
                {
                    server.State = ServerState.Offline;
                    database.SaveChanges();
                }
            }

            foreach (var client in _clients.ToArray())
            {
                Send(client, ServerMessage.Create(ServerId).Bye("Shutdown"));
                RemoveClient(client);
            }

            State = ListenerState.Idle;
            _listener.Close();
        }

        private void Send(IPEndPoint receiver, byte[] message)
        {
            _listener.Send(message, message.Length, receiver);
        }

        private void Send(Client receiver, byte[] message)
        {
            _listener.Send(message, message.Length, receiver.EndPoint);
        }

        public void Send(IPEndPoint receiver, Message message)
        {
            Send(receiver, message.GetBytes());
            MessageSent?.Invoke(receiver, message);
        }

        public void Send(Client receiver, Message message)
        {
            Send(receiver, message.GetBytes());
            MessageSent?.Invoke(receiver.EndPoint, message);
        }

        public void Send(Guid receiverId, Message message)
        {
            var receiver = _clients.FirstOrDefault(c => c.User.Id == receiverId);
            Send(receiver, message.GetBytes());
            MessageSent?.Invoke(receiver.EndPoint, message);
        }

        public void Broadcast(Message message)
        {
            foreach (var client in _clients)
            {
                Send(client, message);
            }
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

                Task.Factory.StartNew(() => HandleMessage(endpoint, Message.FromBytes(msg)));
            }
        }

        public Client GetClient(IPEndPoint endpoint)
        {
            return _clients.FirstOrDefault(c => Equals(c.EndPoint, endpoint));
        }

        public Client GetClient(Guid id)
        {
            return _clients.FirstOrDefault(c => c.User.Id == id);
        }

        public Conversation GetConversation(Client member)
        {
            return _conversations.FirstOrDefault(c => c.IsMember(member));
        }

        public Conversation GetConversation(Guid conversationId)
        {
            return _conversations.FirstOrDefault(c => c.Id == conversationId);
        }

        private void HandleMessage(IPEndPoint endpoint, IMessage message)
        {
            /* If client is not on the client list, and the message is not 'hello' */
            if (message.MessageType != 0 && _clients.All(c => !Equals(c.EndPoint, endpoint)))
            {
                return;
            }

            switch (message.Category)
            {
                case MessageCategory.Server:
                    HandleServerMessage(endpoint, (ServerMessage) message);
                    break;
                case MessageCategory.Conversation:
                    HandleConversationMessage(endpoint, (ConversationMessage) message);
                    break;
                case MessageCategory.Call:
                    HandleCallMessage(endpoint, (CallMessage) message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            NewMessage?.Invoke(endpoint, message);
        }

        private void HandleServerMessage(IPEndPoint endpoint, ServerMessage message)
        {
            var sender = GetClient(message.SenderId);

            switch (message.Type)
            {
                case ServerMessageType.Hello:
                    HandleHello(endpoint, message);
                    break;
                case ServerMessageType.Bye:
                    HandleBye(sender, message);
                    break;
                case ServerMessageType.KeepAlive:
                    HandleKeepAlive(sender, message);
                    break;
                case ServerMessageType.Ok:
                    break;
                case ServerMessageType.Error:
                    break;
                case ServerMessageType.CreateConversation:
                    HandleCreateConversation(sender, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleConversationMessage(IPEndPoint endpoint, ConversationMessage message)
        {
            var conversationId = message.GetConversationId();
            var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conversation == null)
            {
                using (var database = new RozmawiatorDb())
                {
                    if (database.Conversations.Find(conversationId) != null)
                    {
                        conversation = new Conversation(conversationId, this);
                        _conversations.Add(conversation);
                    }
                }
            }

            conversation?.HandleMessage(message);
        }

        private void HandleCallMessage(IPEndPoint endpoint, CallMessage message)
        {
            var callId = message.GetCallId();
            var conversation = _conversations.FirstOrDefault(c => c.Call?.Id == callId);
            if (conversation == null)
            {
                using (var database = new RozmawiatorDb())
                {
                    var call = database.Calls.Find(callId);
                    if (call != null)
                    {
                        conversation = new Conversation(call.ConversationId, this);
                        _conversations.Add(conversation);
                    }
                }
            }

            conversation?.HandleMessage(message);
        }

        private void HandleHello(IPEndPoint endpoint, ServerMessage message)
        {
            var client = GetClient(endpoint);
            if (client != null)
            {
                Debug($"Client with endpoint {endpoint.Address}:{endpoint.Port} already on list ({client.User.Id}).");
                return;
            }

            User user;
            using (var database = new RozmawiatorDb())
            {
                user = database.Users.FirstOrDefault(u => u.Id == message.SenderId);
                if (user == null)
                {
                    Debug($"Client {message.SenderId} with endpoint {endpoint.Address}:{endpoint.Port} does not exist in database.");
                    return;
                }
            }

            client = GetClient(user.Id);
            if (client != null)
            {
                Debug($"Client with endpoint {endpoint.Address}:{endpoint.Port} already on list ({client.User.Id}).");
                Send(endpoint, ServerMessage.Create(ServerId).Bye("User is already connected."));
                return;
            }

            var newClient = AddClient(endpoint, user);
            Send(newClient, ServerMessage.Create(ServerId).Hello());

            Debug($"{newClient.User.Id} connected ({endpoint.Address}:{endpoint.Port})");
            ClientConnected?.Invoke(newClient);

            AddToExistingConversations(newClient);
        }

        private void HandleBye(Client sender, ServerMessage message)
        {
            if (sender == null)
            {
                return;
            }
            Debug($"{sender.User.Id} disconnecting ({sender.EndPoint.Address}:{sender.EndPoint.Port})");
            RemoveClient(sender);
        }

        private void HandleCreateConversation(Client sender, ServerMessage message)
        {
            var conversation = new Conversation(Guid.NewGuid(), this);
            conversation.Join(sender);
            _conversations.Add(conversation);

            Send(sender, ServerMessage.Create(ServerId).Ok(conversation.Id.ToByteArray()));
        }

        private void HandleKeepAlive(Client sender, ServerMessage message)
        {
            //Debug($"{sender.Nickname} is alive ({sender.EndPoint.Address}:{sender.EndPoint.Port})");
            sender?.KeepAlive();
        }

        private Client AddClient(IPEndPoint endpoint, User user)
        {
            var client = new Client(this, user, endpoint);
            client.Timeout += ClientOnTimeout;
            _clients.Add(client);

            return client;
        }

        private void AddToExistingConversations(Client client)
        {
            using (var db = new RozmawiatorDb())
            {
                var user = db.Users.First(u => u.Id == client.Id);

                //var serverConversations = conversations.Where(c => _conversations.FirstOrDefault(sc => sc.Id == c.Id) != null);
                foreach (var serverConversation in _conversations)
                {
                    if (user.Conversations.FirstOrDefault(c => c.Id == serverConversation.Id) == null)
                    {
                        continue;
                    }

                    serverConversation.JoinMember(client);
                }
            }
        }

        private void ClientOnTimeout(Client client)
        {
            Debug($"No keepalives - timeout from client {client.User.Id} - removing");
            RemoveClient(client);
        }

        private void RemoveClient(Client client)
        {
            GetConversation(client)?.Disconnect(client);
            ClientDisconnected?.Invoke(client);
            _clients.Remove(client);
        }

        private void OnConversationParticipantsUpdate(Conversation conversation)
        {
            ConversationUpdate?.Invoke(conversation);
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
