using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Shared;

namespace Rozmawiator.ClientApi
{
    public class CallRequest
    {
        private readonly Client _client;
        public short CallerId { get; }
        public string CallerNickname { get; }
        public Message.CallResponseType? Response { get; private set; }

        public CallRequest(short callerId, string callerNickname, Client client)
        {
            CallerId = callerId;
            CallerNickname = callerNickname;
            _client = client;
        }

        public void Accept()
        {
            //_client.Send(new Message().CallResponse(CallerId, Message.CallResponseType.RequestAccepted));
            Response = Message.CallResponseType.RequestAccepted;
            _client.RespondToRequest(this);
        }

        public void Decline()
        {
            Response = Message.CallResponseType.RequestDenied;
            _client.RespondToRequest(this);
            //_client.Send(new Message().CallResponse(CallerId, Message.CallResponseType.RequestDenied));
        }
    }
}
