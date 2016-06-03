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

        public string Nickname { get; }
        public ClientState State { get; private set; } = ClientState.Disconnected;
        public IPEndPoint ServerEndPoint { get; private set; }

        public Conversation JoinedConversation { get; private set; }

        public ReadOnlyObservableCollection<CallRequest> PendingCallRequests { get; }
        private ObservableCollection<CallRequest> _pendingCallRequests;

        public event Action<Client, Message> Connected;
        public event Action<Client, Message> NewMessage;

        public event Action<Client, CallRequest, Message> NewCallRequest;
        public event Action<Client, Message> NewCallResponse;

        private readonly UdpClient _client;
        private const int KeepAliveSpan = 1000;
        private Timer _keepAliveTimer;
        
        public Client(string nickname)
        {
            Nickname = nickname;
            _client = new UdpClient();

            _pendingCallRequests = new ObservableCollection<CallRequest>();
            PendingCallRequests = new ReadOnlyObservableCollection<CallRequest>(_pendingCallRequests);
        }

        public void Connect(IPEndPoint ipEndPoint)
        {
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
            _client.Send(bytes, bytes.Length);
        }

        public void Send(Message message)
        {
            if (State != ClientState.Connected)
            {
                return;
            }

            var bytes = message.GetBytes();
            _client.Send(bytes, bytes.Length);
        }

        public async Task SendAsync(Message message)
        {
            if (State != ClientState.Connected)
            {
                return;
            }

            var bytes = message.GetBytes();
            await _client.SendAsync(bytes, bytes.Length);
        }

        public void Call(string nickname)
        {
            if (JoinedConversation != null)
            {
                throw new InvalidOperationException("User is already in conversation. Disconnect first.");
            }

            Send(new Message().Call(nickname));
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
                case Message.MessageType.HelloConversation:
                case Message.MessageType.ByeConversation:
                case Message.MessageType.CloseConversation:
                case Message.MessageType.Text:
                case Message.MessageType.Audio:
                    JoinedConversation?.HandleMessage(message);
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

        private void HandleCallRequest(Message message)
        {
            var request = new CallRequest(message.Sender, message.GetTextContent(), this);
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
