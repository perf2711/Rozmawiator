using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Shared;

namespace Rozmawiator.ClientApi
{
    public class PassiveConversation
    {
        public Client Client { get; }
        public List<string> ParticipantsNicknames { get; }
        public IReadOnlyList<Message> TextMessages => _textMessages.AsReadOnly();

        public event Action<PassiveConversation, Message> NewMessage;

        private readonly List<Message> _textMessages;

        public PassiveConversation(Client client)
        {
            Client = client;
            ParticipantsNicknames = new List<string>();
            _textMessages = new List<Message>();
        }

        public void HandleMessage(Message message)
        {
            switch (message.Type)
            {
                case Message.MessageType.DirectText:
                    break;
                default:
                    return;
            }
            _textMessages.Add(message);

            NewMessage?.Invoke(this, message);
        }

        public void Send(string message)
        {
            foreach (var participant in ParticipantsNicknames)
            {
                Client.Send(new Message().DirectText(participant, message));
            }
        }
    }
}
