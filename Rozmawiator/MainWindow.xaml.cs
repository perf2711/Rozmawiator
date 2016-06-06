using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rozmawiator.Audio;
using Rozmawiator.ClientApi;
using Rozmawiator.Controls;
using Rozmawiator.Data;
using Rozmawiator.Database.Entities;
using Rozmawiator.Extensions;
using Rozmawiator.Models;
using Rozmawiator.PartialViews;
using Rozmawiator.Shared;
using Conversation = Rozmawiator.Models.Conversation;
using Message = Rozmawiator.Shared.Message;

namespace Rozmawiator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Initializing

        private async Task UpdateData()
        {
            this.ShowLoading("Aktualizowanie danych...");
            await PassiveConversationService.UpdateConversations();

            Dispatcher.Invoke(UpdateViews);

            this.HideLoading();
        }

        private void UpdateViews()
        {
            foreach (var conversation in PassiveConversationService.Conversations)
            {
                var control = new PassiveConversationControl
                {
                    Conversation = conversation
                };

                PassiveConversationList.Items.Add(control);
            }
        }

        private void SetEvents()
        {
            ClientService.Client.NewMessage += OnNewMessage;
        }

        #endregion

        #region Handling messages

        private void OnNewMessage(Client client, Message message)
        {
            switch (message.Type)
            {
                case Message.MessageType.Hello:
                case Message.MessageType.Bye:
                case Message.MessageType.KeepAlive:
                case Message.MessageType.Call:
                    break;
                case Message.MessageType.DirectText:
                    HandleDirectText(message);
                    break;
                case Message.MessageType.CallRequest:
                case Message.MessageType.CallResponse:
                    break;
                case Message.MessageType.HelloConversation:
                case Message.MessageType.ByeConversation:
                case Message.MessageType.CloseConversation:
                case Message.MessageType.Text:
                case Message.MessageType.Audio:
                    break;
            }
        }

        private async void HandleDirectText(Message message)
        {
            var sender = message.GetDirectTextNickname();
            var content = message.GetDirectTextContent();

            var conversation =
                PassiveConversationService.Conversations.FirstOrDefault(c => c.Participants.Select(p => p.Nickname).Contains(sender)) ??
                await PassiveConversationService.AddConversation(UserService.LoggedUser.Nickname, sender);

            var conversationControl = PassiveConversationControls.FirstOrDefault(p => p.Conversation == conversation);
            if (conversationControl == null)
            {
                conversationControl = new PassiveConversationControl
                {
                    Conversation = conversation
                };
                Dispatcher.Invoke(() =>
                {
                    PassiveConversationList.Items.Add(conversationControl);
                });
            }

            if (SelectedConversation != conversationControl.Conversation)
            {
                conversationControl.Notify();
            }

            var messageDisplay = MessageDisplays.FirstOrDefault(m => m.Conversation == conversation);
            if (messageDisplay == null)
            {
                messageDisplay = new MessageDisplayControl(conversation);
                Dispatcher.Invoke(() =>
                {
                    MessageDisplays.Add(messageDisplay);
                });
                return;
            }

            Dispatcher.Invoke(() =>
            {
                var msg = new TextMessage(content, DateTime.Now, UserService.GetUser(sender));
                conversation.Messages.Add(msg);
                messageDisplay.AddMessageControl(msg);
            });
        }

        #endregion

        #region Conversations

        #region Passive

        private IEnumerable<PassiveConversationControl> PassiveConversationControls
            => PassiveConversationList.Items.OfType<PassiveConversationControl>();

        private List<MessageDisplayControl> MessageDisplays { get; } = new List<MessageDisplayControl>();
        private MessageDisplayControl ActiveMessageDisplay { get; set; }

        #region Selecting conversation

        private Conversation SelectedConversation
        {
            get
            {
                PassiveConversationControl control = null;
                Dispatcher.Invoke(() => control = PassiveConversationList.SelectedItem as PassiveConversationControl);
                return control?.Conversation;
            }
        }

        private void PassiveConversationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (PassiveConversationList.SelectedItem as PassiveConversationControl)?.Unnotify();
            DisplayMessages(SelectedConversation);
        }

        #endregion

        #region Displaying messages

        private void DisplayMessages(Conversation conversation)
        {
            var messageDisplay = MessageDisplays.FirstOrDefault(m => m.Conversation == conversation);
            if (messageDisplay == null)
            {
                messageDisplay = new MessageDisplayControl(conversation);
                MessageDisplays.Add(messageDisplay);
            }

            MessagesPanel.Children.Clear();
            MessagesPanel.Children.Add(messageDisplay);
            ActiveMessageDisplay = messageDisplay;
        }

        #endregion

        #region Sending and receiving messages

        private void MessageInputBox_Sent(ChatInputControl sender, TextMessage message)
        {
            if (ActiveMessageDisplay == null || SelectedConversation == null)
            {
                return;
            }

            SendMessage(SelectedConversation, message);
        }

        private void SendMessage(Conversation conversation, TextMessage message)
        {
            if (ActiveMessageDisplay == null || SelectedConversation == null)
            {
                return;
            }

            foreach (var participant in conversation.Participants)
            {
                ClientService.Client.Send(new Message().DirectText(participant.Nickname, message.Content));
            }

            //conversation.Messages.Add(message);
            //ActiveMessageDisplay.AddMessageControl(message);
        }

        #endregion

        #endregion

        #endregion

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Task(async () => await UpdateData()).Start();
            SetEvents();
        }

        #endregion
    }
}
