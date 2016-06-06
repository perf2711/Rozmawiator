using System;
using System.Net;
using System.Timers;
using Rozmawiator.Shared;

namespace Rozmawiator.Server.Api
{
    public class Client
    {

        public short Id { get; }
        public string Nickname { get; }
        public IPEndPoint EndPoint { get; }
        public Listener Server { get; }

        public event Action<Client> Timeout;

        private Timer _timeoutTimer;

        public Client(Listener server, short id, string nickname, IPEndPoint endPoint)
        {
            Id = id;
            Nickname = nickname;
            EndPoint = endPoint;
            Server = server;
            
            _timeoutTimer = new Timer(server.TimeoutSpan);
            _timeoutTimer.Elapsed += OnTimeout;
            _timeoutTimer.Stop();

            if (server.Timeouts)
            {
                _timeoutTimer.Start();
            }
        }

        private void OnTimeout(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Timeout?.Invoke(this);
            _timeoutTimer.Stop();
        }

        public void KeepAlive()
        {
            _timeoutTimer.Stop();
            _timeoutTimer.Start();
        }

        public void Send(Message message)
        {
            Server.Send(this, message);
        }
    }
}
