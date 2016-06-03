using System;
using System.Collections.Generic;
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
        

        public event Action<Client, Message> Connected;
        public event Action<Client, Message> NewMessage;

        private readonly UdpClient _client;
        private const int KeepAliveSpan = 1000;
        private Timer _keepAliveTimer;
        
        public Client(string nickname)
        {
            Nickname = nickname;
            _client = new UdpClient();
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

                /*
                switch (message.Type)
                {
                    case Message.MessageType.Hello:
                        break;
                    case Message.MessageType.Bye:
                        break;
                    case Message.MessageType.KeepAlive:
                        break;
                    case Message.MessageType.Text:
                        break;
                    case Message.MessageType.Audio:
                        break;
                    case Message.MessageType.Call:
                        break;
                    case Message.MessageType.CallRequest:
                        break;
                    case Message.MessageType.CallResponse:
                        break;
                    case Message.MessageType.HelloConversation:
                        break;
                    case Message.MessageType.ByeConversation:
                        break;
                    case Message.MessageType.CloseConversation:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                */
                Task.Factory.StartNew(() => NewMessage?.Invoke(this, message));
            }
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
