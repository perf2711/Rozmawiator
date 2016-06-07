using System;
using System.Linq;
using System.Net;
using System.Timers;
using Rozmawiator.Database;
using Rozmawiator.Database.Entities;
using Message = Rozmawiator.Communication.Message;

namespace Rozmawiator.Server.Api
{
    public class Client
    {
        public User User { get; }
        public IPEndPoint EndPoint { get; }
        public Listener Server { get; }

        public event Action<Client> Timeout;

        private Timer _timeoutTimer;

        public Client(Listener server, User user, IPEndPoint endPoint)
        {
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
