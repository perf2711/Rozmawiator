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

        public CallRequest(short callerId, string callerNickname, Client client)
        {
            CallerId = callerId;
            CallerNickname = callerNickname;
            _client = client;
        }

        public void Accept()
        {
            _client.Send(new Message().CallResponse(CallerId, Message.CallResponseType.RequestAccepted));
        }

        public void Decline()
        {
            _client.Send(new Message().CallResponse(CallerId, Message.CallResponseType.RequestDenied));
        }
    }
}
