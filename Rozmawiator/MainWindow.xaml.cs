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
using CallRequest = Rozmawiator.Models.CallRequest;
using Conversation = Rozmawiator.Models.Conversation;
using Message = Rozmawiator.Shared.Message;
using User = Rozmawiator.Models.User;

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
            await ActiveConversationService.UpdateConversations();

            Dispatcher.Invoke(UpdateViews);

            this.HideLoading();
        }

        private void UpdateViews()
        {
            foreach (var conversation in PassiveConversationService.Conversations)
            {
                var control = new ConversationControl
                {
                    Conversation = conversation
                };

                ConversationList.Items.Add(control);
            }
        }

        private void SetEvents()
        {
            ClientService.Client.NewMessage += OnNewMessage;
            ClientService.Client.NewCallRequest += OnNewCallRequest;
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
                await ConversationService.AddConversation(ConversationType.Passive, UserService.LoggedUser.Nickname, sender);

            var conversationControl = ConversationControls.FirstOrDefault(p => p.Conversation == conversation);
            if (conversationControl == null)
            {
                conversationControl = new ConversationControl
                {
                    Conversation = conversation
                };
                Dispatcher.Invoke(() =>
                {
                    ConversationList.Items.Add(conversationControl);
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

        #region Calls

        #region Calling

        private void Call(User user)
        {
            ClientService.Client.Call(user.Nickname);

            var conversation = new Conversation
            {
                Type = ConversationType.Active,
                Creator = UserService.LoggedUser,
                Owner = UserService.LoggedUser,
                Participants = ClientService.Client.Conversation.Participants.Select(p => UserService.GetUser(p.Nickname)).ToArray()
            };
            ActiveConversationService.CurrentActiveConversation = conversation;

            StartConversation();
        }

        #endregion

        #region Call requests

        private void DisplayCallRequest(CallRequest callRequest)
        {
            Dispatcher.Invoke(() =>
            {
                var control = new CallControl(callRequest);
                control.Accepted += OnCallRequestAccept;
                control.Denied += OnCallRequestDenied;
                control.Ignored += OnCallRequestIgnored;

                Grid.SetColumnSpan(control, 99);
                Grid.SetRowSpan(control, 99);

                MainGrid.Children.Add(control);
            });
        }

        private void HideCallRequests()
        {
            Dispatcher.Invoke(() =>
            {
                var controls = MainGrid.Children.OfType<CallControl>().ToArray();

                foreach (var control in controls)
                {
                    MainGrid.Children.Remove(control);
                }
            });
        }

        private void OnNewCallRequest(Client client, ClientApi.CallRequest callRequest, Message message)
        {
            var request = new CallRequest
            {
                Id = Guid.Empty,
                CallerId = message.Sender,
                Caller = callRequest.CallerNickname,
                Callee = UserService.LoggedUser.Nickname,
                State = CallRequestState.NoResponse
            };
            DisplayCallRequest(request);
        }

        private void OnCallRequestAccept(CallControl callControl)
        {
            var request =
                ClientService.Client.PendingCallRequests.FirstOrDefault(
                    r => r.CallerId == callControl.CallRequest.CallerId);
            if (request == null)
            {
                return;
            }
            request.Accept();
            HideCallRequests();

            ActiveConversationService.CurrentActiveConversation = callControl.Conversation;
            StartConversation();
        }

        private void OnCallRequestDenied(CallControl callControl)
        {
            var request =
                ClientService.Client.PendingCallRequests.FirstOrDefault(
                    r => r.CallerId == callControl.CallRequest.CallerId);
            if (request == null)
            {
                return;
            }
            request.Decline();
            HideCallRequests();
        }

        private void OnCallRequestIgnored(CallControl callControl)
        {
            HideCallRequests();
        }

        #endregion

        #endregion

        #region Conversations

        private IEnumerable<ConversationControl> ConversationControls => ConversationList.Items.OfType<ConversationControl>();
        private List<MessageDisplayControl> MessageDisplays { get; } = new List<MessageDisplayControl>();
        private MessageDisplayControl ActiveMessageDisplay { get; set; }
        private UserInfoControl ActiveUserInfo { get; set; }

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

        #region Displaying info

        private void DisplayInfo(Conversation conversation)
        {
            var user = conversation.Participants.FirstOrDefault(u => u.Nickname != UserService.LoggedUser.Nickname);
            if (user == null)
            {
                return;
            }

            var userInfo = new UserInfoControl(user);
            userInfo.Called += OnUserCall;

            ActiveUserInfo = userInfo;
            UserInfoGrid.Children.Clear();
            UserInfoGrid.Children.Add(userInfo);
        }

        private void OnUserCall(UserInfoControl userInfoControl)
        {
            Call(userInfoControl.User);
        }

        #endregion

        #region Selecting conversation

        private Conversation SelectedConversation
        {
            get
            {
                ConversationControl control = null;
                Dispatcher.Invoke(() => control = ConversationList.SelectedItem as ConversationControl);
                return control?.Conversation;
            }
        }

        private void ConversationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (ConversationList.SelectedItem as ConversationControl)?.Unnotify();
            DisplayMessages(SelectedConversation);
            DisplayInfo(SelectedConversation);
        }

        #endregion

        #region Sending messages

        private void MessageInputBox_Sent(ChatInputControl sender, TextMessage message)
        {
            if (ActiveMessageDisplay == null || SelectedConversation == null)
            {
                return;
            }

            switch (SelectedConversation.Type)
            {
                case ConversationType.Passive:
                    SendPassiveMessage(SelectedConversation, message);
                    break;
                case ConversationType.Active:
                    SendActiveMessage(SelectedConversation, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Passive
        private void SendPassiveMessage(Conversation conversation, TextMessage message)
        {
            if (ActiveMessageDisplay == null)
            {
                return;
            }

            foreach (var participant in conversation.Participants.Where(p => p.Nickname != UserService.LoggedUser.Nickname))
            {
                ClientService.Client.Send(new Message().DirectText(participant.Nickname, message.Content));
            }

            conversation.Messages.Add(message);
            ActiveMessageDisplay.AddMessageControl(message);
        }

        #endregion

        #region Active

        private void StartConversation()
        {
            var activeConversationView = new ActiveConversationViewControl(ActiveConversationService.CurrentActiveConversation);

            ActiveConversationGrid.Children.Clear();
            ActiveConversationGrid.Children.Add(activeConversationView);
            ActiveConversationRow.Height = new GridLength(2, GridUnitType.Star);

            activeConversationView.MicrophoneToggled += OnMicrophoneToggle;
            activeConversationView.SpeakerToggled += OnSpeakerToggle;
            activeConversationView.HangedUp += OnHangedUp;

            DisplayMessages(ActiveConversationService.CurrentActiveConversation);
        }

        private void EndConversation()
        {
            ClientService.Client.Conversation?.Disconnect();
            ActiveConversationGrid.Children.Clear();

            ActiveConversationRow.Height = GridLength.Auto;
        }

        private void SendActiveMessage(Conversation conversation, TextMessage message)
        {
            ClientService.Client.Conversation.SendToAll(Message.CreateNew.Text(message.Content));
        }

        private void OnMicrophoneToggle(ActiveConversationViewControl activeConversationViewControl, bool b)
        {
            //TODO: Muting microphone
        }

        private void OnSpeakerToggle(ActiveConversationViewControl activeConversationViewControl, bool b)
        {
            //TODO: Muting sound
        }

        private void OnHangedUp(ActiveConversationViewControl activeConversationViewControl)
        {
            EndConversation();
        }

        #endregion

        #endregion

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Task(async () => await UpdateData()).Start();
            SetEvents();
            LoggedUserInfoControl.User = UserService.LoggedUser;
        }

        #endregion
    }
}
