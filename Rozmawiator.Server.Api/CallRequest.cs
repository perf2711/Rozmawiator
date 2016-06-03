using Rozmawiator.Shared;

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

        public Listener Server { get; }
        public Client RequestingClient { get; }
        public Client TargetClient { get; }

        public CallRequest(Listener server, Client requestingClient, Client targetClient)
        {
            Server = server;
            RequestingClient = requestingClient;
            TargetClient = targetClient;
        }

        public void SendRequest()
        {
            Server.SendAsClient(RequestingClient, TargetClient, new Message(Message.MessageType.CallRequest, RequestingClient.Nickname));
            State = RequestState.Sent;
        }

        public Message.CallResponseType ResolveRequest(Message message)
        {
            State = RequestState.Resolved;
            return (Message.CallResponseType) message.Content[0];
        }
    }
}
