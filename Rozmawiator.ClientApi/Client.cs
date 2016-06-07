﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Communication.Server;
using Rozmawiator.Request;

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

        public Guid Id { get; set; }
        public ClientState State { get; private set; } = ClientState.Disconnected;
        public IPEndPoint ServerEndPoint { get; private set; }

        public ReadOnlyObservableCollection<Conversation> Conversations { get; }
        private readonly ObservableCollection<Conversation> _conversations;

        public List<ExpectedMessage> ExpectedMessages { get; }

        public event Action<Client, ServerMessage> Connected;
        public event Action<Client, Conversation> NewConversation;

        private readonly UdpClient _client;
        private const int KeepAliveSpan = 1000;
        private Timer _keepAliveTimer;
        
        public Client()
        {
            _client = new UdpClient();

            _conversations = new ObservableCollection<Conversation>();
            Conversations = new ReadOnlyObservableCollection<Conversation>(_conversations);

            ExpectedMessages = new List<ExpectedMessage>();
        }

        public void Connect(IPEndPoint ipEndPoint)
        {
            if (Id == Guid.Empty)
            {
                throw new InvalidOperationException("GUID cannot be zeroes.");
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

            ExpectedMessages.Add(new ExpectedMessage(ServerMessageType.Hello, OnConnected));
            ForceSend(ServerMessage.Create(Id).Hello());
        }

        private void OnConnected(ExpectedMessage expectedMessage, IMessage message)
        {
            State = ClientState.Connected;
            Connected?.Invoke(this, (ServerMessage) message);
        }

        public void Disconnect()
        {
            if (State != ClientState.Connected)
            {
                return;
            }

            Send(ServerMessage.Create(Id).Bye("Disconnect"));
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

        public void CreateConversation()
        {
            ExpectedMessages.Add(new ExpectedMessage(ServerMessageType.Ok, OnConversationCreated));
            Send(ServerMessage.Create(Id).CreateConversation());
        }

        public void LoadConversation(Guid conversationId, params Guid[] participants)
        {
            var conversation = new Conversation(conversationId, this, participants);
            _conversations.Add(conversation);
            NewConversation?.Invoke(this, conversation);
        }

        private void OnConversationCreated(ExpectedMessage expectedMessage, IMessage message)
        {
            var conversationId = ((ConversationMessage) message).GetGuidContent();
            var conversation = new Conversation(conversationId, this);
            _conversations.Add(conversation);
            NewConversation?.Invoke(this, conversation);
        }

        private void ReceiveLoop()
        {
            while (State == ClientState.Connecting)
            {
                var from = new IPEndPoint(0, 0);
                var msg = _client.Receive(ref from);

                var message = Message.FromBytes(msg);
                CheckExpectedMessages(message);
            }

            while (State == ClientState.Connected)
            {
                var from = new IPEndPoint(0, 0);
                var msg = _client.Receive(ref from);

                var message = Message.FromBytes(msg);
                Task.Factory.StartNew(() => HandleMessage(message));
            }
        }

        private void CheckExpectedMessages(IMessage message)
        {
            foreach (var expectedMessage in ExpectedMessages.ToArray())
            {
                if (expectedMessage.Test(message))
                {
                    ExpectedMessages.Remove(expectedMessage);
                }
            }
        }

        private void HandleMessage(IMessage message)
        {
            CheckExpectedMessages(message);

            switch (message.Category)
            {
                case MessageCategory.Server:
                    HandleServerMessage((ServerMessage)message);
                    break;
                case MessageCategory.Conversation:
                    HandleConversationMessage((ConversationMessage)message);
                    break;
                case MessageCategory.Call:
                    HandleCallMessage((CallMessage)message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleServerMessage(ServerMessage message)
        { 
            switch (message.Type)
            {
                case ServerMessageType.Hello:
                case ServerMessageType.KeepAlive:
                case ServerMessageType.CreateConversation:
                case ServerMessageType.Ok:
                case ServerMessageType.Error:
                    break;
                case ServerMessageType.Bye:
                    HandleBye(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleConversationMessage(ConversationMessage message)
        {
            var conversationId = message.GetConversationId();
            var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);

            if (conversation == null)
            {
                conversation = new Conversation(conversationId, this);
                _conversations.Add(conversation);
                NewConversation?.Invoke(this, conversation);
            }

            conversation.HandleConversationMessage(message);

        }

        private void HandleCallMessage(CallMessage message)
        {
            var callId = message.GetCallId();
            var call = _conversations.Select(c => c.Call).FirstOrDefault(c => c?.Id == callId);

            call?.HandleCallMessage(message);
        }

        private void HandleBye(ServerMessage message)
        {
            Disconnect();
        }

        private void SendHeartbeat(object state)
        {
            if (State != ClientState.Connected)
            {
                return;
            }

            Send(ServerMessage.Create(Id).KeepAlive());
        }
    }
}
