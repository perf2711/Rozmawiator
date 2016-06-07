using Rozmawiator.Communication;
using Rozmawiator.Communication.Conversation;

namespace Rozmawiator.Server.Api
{
    public class CallRequest
    {
        public enum RequestState
        {
            Untouched,
            Sent,
            Resolved
        }

        public RequestState State { get; private set; } = RequestState.Untouched;
        public Conversation Conversation { get; }

        public CallRequest(Conversation conversation)
        { 
            Conversation = conversation;
        }

        public void SendRequest()
        {
            //Server.SendAsClient(RequestingClient, TargetClient, new Message(Message.MessageType.CallRequest, RequestingClient.Nickname));
            State = RequestState.Sent;
        }

        public CallResponseType ResolveRequest(Message message)
        {
            State = RequestState.Resolved;
            return (CallResponseType) message.Content[0];
        }
    }
}
