using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rozmawiator.Shared;

namespace Rozmawiator.ClientApi
{
    public class Client
    {
        public enum ClientState
        {
            Disconnected,
            Connecting,
            Connected
        }

        public string Nickname { get; set; }
        public ClientState State { get; private set; } = ClientState.Disconnected;
        public IPEndPoint ServerEndPoint { get; private set; }

        public Conversation Conversation { get; private set; }

        public ReadOnlyObservableCollection<CallRequest> PendingCallRequests { get; }
        private readonly ObservableCollection<CallRequest> _pendingCallRequests;

        public List<PassiveConversation> PassiveConversations { get; }

        public event Action<Client, Message> Connected;
        public event Action<Client, Message> NewMessage;

        public event Action<Client, CallRequest, Message> NewCallRequest;
        public event Action<Client, Message> NewCallResponse;

        public event Action<Client, PassiveConversation> NewPassiveConversation;

        private readonly UdpClient _client;
        private const int KeepAliveSpan = 1000;
        private Timer _keepAliveTimer;
        
        public Client()
        {
            _client = new UdpClient();

            _pendingCallRequests = new ObservableCollection<CallRequest>();
            PendingCallRequests = new ReadOnlyObservableCollection<CallRequest>(_pendingCallRequests);

            PassiveConversations = new List<PassiveConversation>();
        }

        public void Connect(IPEndPoint ipEndPoint)
        {
            if (string.IsNullOrEmpty(Nickname))
            {
                throw new InvalidOperationException("Nickname cannot be empty.");
            }

            if (State != ClientState.Disconnected)
            {
                return;
            }

            ServerEndPoint = ipEndPoint;
            _client.Connect(ipEndPoint);
            State = ClientState.Connecting;

            _keepAliveTimer = new Timer(SendHeartbeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(KeepAliveSpan));
            Task.Factory.StartNew(ReceiveLoop);

            ForceSend(Message.Hello(Nickname));
        }

        public void Disconnect()
        {
            if (State != ClientState.Connected)
            {
                return;
            }

            Send(Message.Bye());
            State = ClientState.Disconnected;
            _client.Close();
            ServerEndPoint = null;
        }

        private void ForceSend(Message message)
        {
            var bytes = message.GetBytes();
            message.Origin = Message.MessageOrigin.Sent;
            _client.Send(bytes, bytes.Length);
        }

        public void Send(Message message)
        {
            if (State != ClientState.Connected)
            {
                return;
            }

            var bytes = message.GetBytes();
            message.Origin = Message.MessageOrigin.Sent;
            _client.Send(bytes, bytes.Length);
        }

        public async Task SendAsync(Message message)
        {
            if (State != ClientState.Connected)
            {
                return;
            }

            var bytes = message.GetBytes();
            message.Origin = Message.MessageOrigin.Sent;
            await _client.SendAsync(bytes, bytes.Length);
        }

        public void Call(string nickname)
        {
            JoinNewConversation();
            Conversation.AddUser(nickname);
        }

        public void DisconnectFromConversation(string reason)
        {
            Conversation.SendToAll(Message.CreateNew.ByeConversation(reason));
            Conversation = null;
        }

        public void RespondToRequest(CallRequest request)
        {
            if (request.Response == null)
            {
                //Ignored
                _pendingCallRequests.Remove(request);
                return;
            }

            switch (request.Response.Value)
            {
                case Message.CallResponseType.RequestDenied:
                case Message.CallResponseType.RequestAccepted:
                    JoinNewConversation();
                    Send(new Message().CallResponse(request.CallerId, request.Response.Value));
                    break;
                default:
                    throw new InvalidOperationException("Invalid response. Client can only send Accepted and Denied.");
            }

            _pendingCallRequests.Remove(request);
        }

        public void JoinNewConversation()
        {
            Conversation?.Disconnect();
            Conversation = new Conversation(this);
        }

        private void ReceiveLoop()
        {
            while (State == ClientState.Connecting)
            {
                var from = new IPEndPoint(0, 0);
                var msg = _client.Receive(ref from);

                var message = Message.FromBytes(msg);
                if (message.Type == Message.MessageType.Hello && message.Sender == -1)
                {
                    State = ClientState.Connected;
                    Connected?.Invoke(this, message);
                    break;
                }
            }

            while (State == ClientState.Connected)
            {
                var from = new IPEndPoint(0, 0);
                var msg = _client.Receive(ref from);

                var message = Message.FromBytes(msg);
                message.Origin = Message.MessageOrigin.Received;
                Task.Factory.StartNew(() => HandleMessage(message));
            }
        }

        private void HandleMessage(Message message)
        {
            switch (message.Type)
            {
                case Message.MessageType.Hello:
                case Message.MessageType.Bye:
                case Message.MessageType.KeepAlive:
                case Message.MessageType.Call:
                    break;
                case Message.MessageType.DirectText:
                    HandleDirectText(message);
                    break;
                case Message.MessageType.HelloConversation:
                case Message.MessageType.ByeConversation:
                case Message.MessageType.CloseConversation:
                case Message.MessageType.Text:
                case Message.MessageType.Audio:
                    Conversation?.HandleMessage(message);
                    break;
                case Message.MessageType.CallRequest:
                    HandleCallRequest(message);
                    break;
                case Message.MessageType.CallResponse:
                    NewCallResponse?.Invoke(this, message);
                    break;
            }

            NewMessage?.Invoke(this, message);
        }


        private void HandleDirectText(Message message)
        {
            var senderNickname = message.GetDirectTextNickname();
            var passiveConversation = PassiveConversations.FirstOrDefault(p => p.ParticipantsNicknames.Any(n => n == senderNickname));

            if (passiveConversation == null)
            {
                passiveConversation = new PassiveConversation(this);
                PassiveConversations.Add(passiveConversation);
                passiveConversation.ParticipantsNicknames.Add(senderNickname);
                NewPassiveConversation?.Invoke(this, passiveConversation);
            }

            passiveConversation.HandleMessage(message);
        }

        private void HandleCallRequest(Message message)
        {
            var request = new CallRequest(message.Sender, message.GetTextContent(), this);
            _pendingCallRequests.Add(request);
            NewCallRequest?.Invoke(this, request, message);
        }

        private void SendHeartbeat(object state)
        {
            if (State != ClientState.Connected)
            {
                return;
            }

            Send(Message.KeepAlive());
        }
    }
}
