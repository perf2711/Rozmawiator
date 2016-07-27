using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Rozmawiator.Audio;
using Rozmawiator.ClientApi;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Communication.Server;
using Rozmawiator.Controls;
using Rozmawiator.Data;
using Rozmawiator.Database.Entities;
using Rozmawiator.Extensions;
using Rozmawiator.Models;
using Rozmawiator.PartialViews;
using Rozmawiator.Windows;
using Call = Rozmawiator.Models.Call;
using CallRequest = Rozmawiator.Models.CallRequest;
using Conversation = Rozmawiator.Models.Conversation;
using Message = Rozmawiator.Communication.Message;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoggedUserInfoControl.User = UserService.LoggedUser;
            SetEvents();
            new Task(async () => await UpdateData()).Start();
            InitializeSoundDevices(Properties.Settings.Default.PlayDeviceId, Properties.Settings.Default.RecordDeviceId);
        }

        private void SetEvents()
        {
            ClientService.Client.NewConversation += OnNewConversation;
            ClientService.Client.DisconnectedByServer += ClientOnDisconnectedByServer;
        }

        private void ClientOnDisconnectedByServer(Client client, ServerMessage serverMessage)
        {
            Dispatcher.Invoke(() =>
            {
                this.ShowError("Uwaga", $"Odłączono od serwera. Przyczyna: {serverMessage.GetStringContent()}", (e) => Logout());
            });
        }

        private void Logout()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private async Task UpdateData()
        {
            this.ShowLoading("Aktualizowanie danych...");
            await ConversationService.UpdateConversations();
            LoadClientConversations();

            this.HideLoading();
        }

        private void LoadClientConversations()
        {
            foreach (var conversation in ConversationService.Conversations)
            {
                ClientService.Client.LoadConversation(conversation.Id, conversation.Participants.Select(p => p.Id).ToArray());
            }
        }

        #endregion

        #region Conversations

        private List<ConversationViewControl> ConversationViews { get; } = new List<ConversationViewControl>();

        private ConversationControl SelectedConversation
        {
            get
            {
                var conversationControl = ConversationList.SelectedItem as ConversationControl;
                return conversationControl;
            }
        }

        private ConversationControl GetConversationControl(Conversation conversation)
        {
            return
                ConversationList.Items.OfType<ConversationControl>().FirstOrDefault(c => c.Conversation == conversation);
        }

        private ConversationViewControl GetConversationView(Conversation conversation)
        {
            return
                ConversationViews.FirstOrDefault(v => v.Conversation.Id == conversation.Id);
        }

        private ConversationControl GetConversationControl(Guid id)
        {
            return
                ConversationList.Items.OfType<ConversationControl>().FirstOrDefault(c => c.Conversation.Id == id);
        }

        private ConversationViewControl GetConversationView(Guid id)
        {
            return
                ConversationViews.FirstOrDefault(v => v.Conversation.Id == id);
        }

        private void ConversationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var conversation = SelectedConversation.Conversation;

            if (ConversationViews.All(m => m.Conversation != conversation))
            {
                var display = new ConversationViewControl(conversation);
                SetConversationViewEvents(display);
                ConversationViews.Add(display);
            }

            ConversationGrid.Children.Clear();

            var displays = ConversationViews.Where(m => m.Conversation == conversation);
            foreach (var display in displays)
            {
                ConversationGrid.Children.Add(display);
            }
        }

        private async void OnNewConversation(Client client, ClientApi.Conversation conversation)
        {
            if (ConversationService.Conversations == null)
            {
                return;
            }

            var conversationModel = ConversationService.Conversations.FirstOrDefault(c => c.Id == conversation.Id) ??
                                    //await ConversationService.AddConversation(conversation.Participants.ToArray());
                                    await ConversationService.AddConversation(conversation);

            Dispatcher.Invoke(() =>
            {
                var control = new ConversationControl { Conversation = conversationModel };
                ConversationList.Items.Add(control);
                SetConversationEvents(conversation);
            });
        }

        private void SetConversationEvents(ClientApi.Conversation conversation)
        {
            conversation.NewTextMessage += ConversationOnNewTextMessage;
            conversation.NewCallRequest += ConversationOnNewCallRequest;
            conversation.CallRequestRevoked += ConversationOnCallRequestRevoked;
            conversation.NewCall += ConversationOnNewCall;
            conversation.ParticipantConnected += ConversationOnParticipantConnected;
            conversation.ParticipantDisconnected += ConversationOnParticipantDisconnected;
        }

        private void SetConversationViewEvents(ConversationViewControl conversationView)
        {
            conversationView.Sent += ConversationViewOnSent;
            conversationView.Called += ConversationViewOnCalled;
            conversationView.HangedUp += ConversationViewOnHangedUp;
            conversationView.MicrophoneToggled += ConversationViewOnMicrophoneToggled;
            conversationView.SpeakerToggled += ConversationViewOnSpeakerToggled;
        }

        private void SetCallRequestControlEvents(CallRequestControl callRequestControl)
        {
            callRequestControl.Accepted += CallRequestControlOnAccepted;
            callRequestControl.Denied += CallRequestControlOnDenied;
            callRequestControl.Ignored += CallRequestControlOnIgnored;
        }

        private void SetCallEvents(ClientApi.Call call)
        {
            call.NewParticipant += CallOnNewParticipant;
            call.ParticipantLeft += CallOnParticipantLeft;
            call.ParticipantDeclined += CallOnParticipantDeclined;
            call.NewAudio += CallOnNewAudio;
            call.CallEnded += CallOnCallEnded;
        }

        private void ConversationViewOnSent(ConversationViewControl conversationViewControl, TextMessage textMessage)
        {
            var conversation =
                ClientService.Client.Conversations.FirstOrDefault(c => c.Id == conversationViewControl.Conversation.Id);

            conversation?.Send(
                ConversationMessage.Create(ClientService.Client.Id, conversation.Id).Text(textMessage.Content));
        }

        private void ConversationViewOnCalled(ConversationViewControl conversationViewControl)
        {
            var conversation =
                ClientService.Client.Conversations.FirstOrDefault(c => c.Id == conversationViewControl.Conversation.Id);

            conversation?.CreateCall();
        }

        private void ConversationViewOnHangedUp(ConversationViewControl conversationViewControl)
        {
            var conversation =
                ClientService.Client.Conversations.FirstOrDefault(c => c.Id == conversationViewControl.Conversation.Id);

            conversation?.DisconnectCall();
            conversationViewControl.HideCall();
        }

        private void ConversationViewOnMicrophoneToggled(ConversationViewControl conversationViewControl, bool state)
        {
            if (state)
            {
                Recorder.Start();
            }
            else
            {
                Recorder.Stop();
            }
        }

        private void ConversationViewOnSpeakerToggled(ConversationViewControl conversationViewControl, bool state)
        {
            Player.SetMute(!state);
        }

        private void ConversationOnNewTextMessage(ClientApi.Conversation conversation, ConversationMessage conversationMessage)
        {
            Dispatcher.Invoke(() =>
            {
                var conversationModel = ConversationService.Conversations.First(c => c.Id == conversation.Id);
                var conversationControl = GetConversationControl(conversationModel);

                if (SelectedConversation.Conversation.Id != conversationControl.Conversation.Id)
                {
                    conversationControl.Notify();
                }

                var textMessage = new TextMessage(conversationMessage.GetStringContent(), DateTime.Now,
                    UserService.Users.FirstOrDefault(u => u.Id == conversationMessage.SenderId));

                conversationModel.Messages.Add(textMessage);

                var conversationDisplay = ConversationViews.FirstOrDefault(m => m.Conversation == conversationModel);
                conversationDisplay?.AddMessage(textMessage);
            });
        }

        private void ConversationOnNewCallRequest(ClientApi.Conversation conversation, ClientApi.CallRequest callRequest)
        {
            Dispatcher.Invoke(() =>
            {
                var conversationModel = ConversationService.Conversations.First(c => c.Id == conversation.Id);
                var callRequestModel = new CallRequest
                {
                    Id = callRequest.CallId,
                    Conversation = conversationModel,
                    Timestamp = DateTime.Now,
                    ClientCallRequest = callRequest
                };

                var control = new CallRequestControl(callRequestModel);
                SetCallRequestControlEvents(control);

                Grid.SetColumnSpan(control, 99);
                Grid.SetRowSpan(control, 99);

                MainGrid.Children.Add(control);
            });
        }

        private void ConversationOnCallRequestRevoked(ClientApi.Conversation conversation, Guid guid)
        {
            Dispatcher.Invoke(() =>
            {
                var callRequestControl =
                    MainGrid.Children.OfType<CallRequestControl>()
                        .FirstOrDefault(c => c.CallRequest.ClientCallRequest.CallId == guid);

                MainGrid.Children.Remove(callRequestControl);
            });
        }

        private void ConversationOnNewCall(ClientApi.Conversation conversation, ClientApi.Call call)
        {
            Dispatcher.Invoke(() =>
            {
                var conversationModel = ConversationService.Conversations.First(c => c.Id == conversation.Id);
                var view = GetConversationView(conversationModel);

                if (conversationModel.Call != null)
                {
                    return;
                }

                conversationModel.Call = new Call
                {
                    Id = call.Id,
                    Participants = call.Participants.Select(p => UserService.Users.FirstOrDefault(u => u.Id == p)).ToList(),
                    Conversation = conversationModel
                };

                view?.ShowCall(conversationModel.Call);
                foreach (var participant in conversationModel.Participants.Where(p => p.Id != UserService.LoggedUser.Id))
                {
                    view?.AddTemporaryUser(participant, UserThumbnailControl.CallState.Calling);
                }

                CallService.CurrentCall = conversationModel.Call;
                SetCallEvents(call);

                Recorder.Start();
                Player.Start();
            });
        }

        private async void ConversationOnParticipantConnected(ClientApi.Conversation conversation, Guid guid)
        {
            await ConversationService.UpdateConversation(conversation.Id);
            Dispatcher.Invoke(() =>
            {
                var conversationControl = GetConversationControl(conversation.Id);
                conversationControl?.Update();

                var conversationView = GetConversationView(conversation.Id);
                conversationView?.Update();
            });
        }

        private async void ConversationOnParticipantDisconnected(ClientApi.Conversation conversation, Guid guid)
        {
            await ConversationService.UpdateConversation(conversation.Id);

            Dispatcher.Invoke(() =>
            {
                var conversationControl = GetConversationControl(conversation.Id);
                conversationControl?.Update();

                var conversationView = GetConversationView(conversation.Id);
                conversationView?.Update();
            });
        }

        private void CallRequestControlOnAccepted(CallRequestControl callRequestControl)
        {
            callRequestControl.CallRequest.ClientCallRequest.Accept();
            MainGrid.Children.Remove(callRequestControl);
        }

        private void CallRequestControlOnDenied(CallRequestControl callRequestControl)
        {
            callRequestControl.CallRequest.ClientCallRequest.Decline();
            MainGrid.Children.Remove(callRequestControl);
        }

        private void CallRequestControlOnIgnored(CallRequestControl callRequestControl)
        {
            callRequestControl.CallRequest.ClientCallRequest.Ignore();
            MainGrid.Children.Remove(callRequestControl);
        }

        private void CallOnNewParticipant(ClientApi.Call call, Guid guid)
        {
            Dispatcher.Invoke(() =>
            {
                var conversationModel = ConversationService.Conversations.First(c => c.Id == call.Conversation.Id);
                var view = GetConversationView(conversationModel);
                var user = UserService.Users.FirstOrDefault(u => u.Id == guid);

                view?.CallView.Call?.Participants.Add(user);
                view?.CallView?.Update();
                view?.RemoveTemporaryUser(user);
            });
        }

        private void CallOnParticipantLeft(ClientApi.Call call, Guid guid)
        {
            Dispatcher.Invoke(() =>
            {
                var conversationModel = ConversationService.Conversations.First(c => c.Id == call.Conversation.Id);
                var view = GetConversationView(conversationModel);
                var user = UserService.Users.FirstOrDefault(u => u.Id == guid);

                view?.CallView.Call?.Participants.Remove(user);
                view?.CallView?.Update();
                view?.RemoveTemporaryUser(user);
            });
        }

        private void CallOnParticipantDeclined(ClientApi.Call call, Guid guid)
        {
            Dispatcher.Invoke(() =>
            {
                var conversationModel = ConversationService.Conversations.First(c => c.Id == call.Conversation.Id);
                var view = GetConversationView(conversationModel);
                var user = UserService.Users.FirstOrDefault(u => u.Id == guid);

                view?.RemoveTemporaryUser(user);
            });
        }

        private void CallOnNewAudio(ClientApi.Call call, CallMessage callMessage)
        {
            Player.AddSamples(callMessage.Content, 0, callMessage.Content.Length);
        }
        
        private void CallOnCallEnded(ClientApi.Call call)
        {
            Dispatcher.Invoke(() =>
            {
                var conversationModel = ConversationService.Conversations.First(c => c.Id == call.Conversation.Id);
                conversationModel.Call = null;
                CallService.CurrentCall = null;

                var view = ConversationViews.FirstOrDefault(c => c.Call.Id == call.Id);
                view?.HideCall();

                Recorder.Stop();
                Player.Stop();
            });
        }

        private void OnResultUserAddToConversation(SearchUsersControl search, User user)
        {
            if (SelectedConversation == null)
            {
                return;
            }

            var conversation =
                ClientService.Client.Conversations.FirstOrDefault(c => c.Id == SelectedConversation.Conversation.Id);

            conversation?.AddUser(user.Id);
        }

        private void OnResultUserAddToNewConversation(SearchUsersControl arg1, User user)
        {
            ClientService.Client.CreateConversation((con) =>
            {
                con.AddUser(user.Id);
            });
        }

        #endregion


        #region Audio

        private void InitializeSoundDevices(int playDeviceId = -1, int recordDeviceId = -1)
        {
            Recorder = recordDeviceId == -1 ? new Recorder() : new Recorder(recordDeviceId);
            Player = playDeviceId == -1 ? new Player() : new Player(playDeviceId);

            Recorder.DataAvailable += OnRecorderDataAvailable;
        }

        #region Recording

        private Recorder Recorder { get; set; }

        private void OnRecorderDataAvailable(Recorder recorder, byte[] bytes)
        {
            if (CallService.CurrentCall == null)
            {
                //return;
                Recorder.Stop();
                return;
            }

            var call =
                ClientService.Client.Conversations.Select(c => c.Call)
                    .FirstOrDefault(c => c?.Id == CallService.CurrentCall.Id);
            call?.Send(CallMessage.Create(UserService.LoggedUser.Id, call.Id).Audio(bytes));
        }

        #endregion

        #region Playing

        private Player Player { get; set; }


        #endregion

        #endregion

        #region Menu

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();

            if (settingsWindow.SettingsChanged)
            {
                InitializeSoundDevices(Properties.Settings.Default.PlayDeviceId, Properties.Settings.Default.RecordDeviceId);
            }
        }

        #endregion

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }
    }
}
