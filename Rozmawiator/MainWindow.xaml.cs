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

            Title = $"Rozmawiator - {UserService.LoggedUser.Nickname}";
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
                    PassiveConversation = conversation
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

            var conversationControl = ConversationControls.FirstOrDefault(p => p.PassiveConversation == conversation);
            if (conversationControl == null)
            {
                conversationControl = new ConversationControl
                {
                    PassiveConversation = conversation
                };
                Dispatcher.Invoke(() =>
                {
                    ConversationList.Items.Add(conversationControl);
                });
            }

            if (SelectedConversationControl.PassiveConversation != conversationControl.PassiveConversation)
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
            SelectedConversationControl.ActiveConversation = conversation;
            

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
        private MessageDisplayControl CurrentMessageDisplay { get; set; }
        private ActiveConversationViewControl CurrentActiveConversationView { get; set; }
        private UserInfoControl CurrentUserInfo { get; set; }

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
            CurrentMessageDisplay = messageDisplay;
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

            CurrentUserInfo = userInfo;
            UserInfoGrid.Children.Clear();
            UserInfoGrid.Children.Add(userInfo);
        }

        private void OnUserCall(UserInfoControl userInfoControl)
        {
            Call(userInfoControl.User);
        }

        #endregion

        #region Displaying active conversation view

        private void DisplayActiveConversationView(Conversation activeConversation)
        {
            if (CurrentActiveConversationView == null)
            {
                CurrentActiveConversationView = new ActiveConversationViewControl(activeConversation);
                CurrentActiveConversationView.MicrophoneToggled += OnMicrophoneToggle;
                CurrentActiveConversationView.SpeakerToggled += OnSpeakerToggle;
                CurrentActiveConversationView.HangedUp += OnHangedUp;
            }
            else if (CurrentActiveConversationView?.Conversation != activeConversation)
            {
                throw new InvalidOperationException("Cannot show new conversation when there is already one in progress.");
            }

            ActiveConversationGrid.Children.Clear();
            ActiveConversationGrid.Children.Add(CurrentActiveConversationView);
            ActiveConversationRow.Height = new GridLength(2, GridUnitType.Star);
        }

        #endregion

        #region Selecting conversation

        private ConversationControl SelectedConversationControl
        {
            get
            {
                ConversationControl control = null;
                Dispatcher.Invoke(() => control = ConversationList.SelectedItem as ConversationControl);
                return control;
            }
        }

        private void ConversationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (ConversationList.SelectedItem as ConversationControl)?.Unnotify();
            DisplayMessages(SelectedConversationControl.PassiveConversation);

            if (SelectedConversationControl.ActiveConversation == null)
            {
                DisplayInfo(SelectedConversationControl.PassiveConversation);
                return;
            }

            UserInfoGrid.Children.Clear();
            CurrentUserInfo = null;
            DisplayActiveConversationView(SelectedConversationControl.ActiveConversation);
        }

        private void SelectConversationByPassive(Conversation conversation)
        {
            var controls = ConversationList.Items.OfType<ConversationControl>();

            var control =
                controls.FirstOrDefault(c => c.PassiveConversation.Participants.SequenceEqual(conversation.Participants));

            if (control == null)
            {
                return;
            }

            ConversationList.SelectedItem = control;
        }

        private void SelectConversationByActive(Conversation conversation)
        {
            var controls = ConversationList.Items.OfType<ConversationControl>();

            var control =
                controls.FirstOrDefault(c => c.ActiveConversation.Participants.SequenceEqual(conversation.Participants));

            if (control == null)
            {
                return;
            }

            ConversationList.SelectedItem = control;
        }

        #endregion

        #region Sending messages

        private void MessageInputBox_Sent(ChatInputControl sender, TextMessage message)
        {
            if (CurrentMessageDisplay == null || SelectedConversationControl == null)
            {
                return;
            }

            SendPassiveMessage(SelectedConversationControl.PassiveConversation, message);
        }

        #endregion

        #region Passive
        private void SendPassiveMessage(Conversation conversation, TextMessage message)
        {
            if (CurrentMessageDisplay == null)
            {
                return;
            }

            foreach (var participant in conversation.Participants.Where(p => p.Nickname != UserService.LoggedUser.Nickname))
            {
                ClientService.Client.Send(new Message().DirectText(participant.Nickname, message.Content));
            }

            conversation.Messages.Add(message);
            CurrentMessageDisplay.AddMessageControl(message);
        }

        #endregion

        #region Active

        private void StartConversation()
        {
            //DisplayMessages();
            SelectConversationByPassive(ActiveConversationService.CurrentActiveConversation);
            SelectedConversationControl.ActiveConversation = ActiveConversationService.CurrentActiveConversation;
            UserInfoGrid.Children.Clear();
            CurrentUserInfo = null;
            DisplayActiveConversationView(SelectedConversationControl.ActiveConversation);

            ClientService.Client.Conversation.ConversationClosed += OnConversationClose;
            ClientService.Client.Conversation.NewAudioMessage += OnNewAudioSamples;

            Recorder.Start();
            Player.Start();
        }

        private void OnNewAudioSamples(ClientApi.Conversation conversation, RemoteClient remoteClient, Message message)
        {
            Player.AddSamples(message.Content, 0, message.Content.Length);
        }

        private void OnConversationClose(ClientApi.Conversation conversation, Message message)
        {
            Dispatcher.Invoke(EndConversation);
        }

        private void EndConversation()
        {
            Player.Stop();
            Recorder.Stop();

            ClientService.Client.Conversation?.Disconnect();
            ActiveConversationGrid.Children.Clear();

            ActiveConversationService.CurrentActiveConversation = null;
            if (SelectedConversationControl.ActiveConversation != null)
            {
                SelectedConversationControl.ActiveConversation = null;
                ConversationSelectionChanged(ConversationList, null);
            }

            ActiveConversationRow.Height = GridLength.Auto;
            CurrentActiveConversationView = null;
        }

        private void SendActiveMessage(Conversation conversation, TextMessage message)
        {
            //ClientService.Client.Conversation.SendToAll(Message.CreateNew.Text(message.Content));
            throw new InvalidOperationException("Deprecated");
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

        #region Audio

        private void InitializeSoundDevices()
        {
            Recorder = new Recorder();
            Player = new Player();

            Recorder.DataAvailable += OnRecorderDataAvailable;
        }

        #region Recording

        private Recorder Recorder { get; set; }

        private void OnRecorderDataAvailable(Recorder recorder, byte[] bytes)
        {
            if (ActiveConversationService.CurrentActiveConversation == null)
            {
                //return;
                Recorder.Stop();
            }

            ClientService.Client.Conversation?.SendToAll(Message.CreateNew.Audio(bytes));
        }

        #endregion

        #region Playing

        private Player Player { get; set; }

        #endregion

        #endregion

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Task(async () => await UpdateData()).Start();
            SetEvents();
            InitializeSoundDevices();
        }

        #endregion
    }
}
