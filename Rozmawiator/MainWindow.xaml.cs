﻿using System;
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
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Controls;
using Rozmawiator.Data;
using Rozmawiator.Database.Entities;
using Rozmawiator.Extensions;
using Rozmawiator.Models;
using Rozmawiator.PartialViews;
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
            InitializeSoundDevices();
        }

        private void SetEvents()
        {
            ClientService.Client.NewConversation += OnNewConversation;
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
            var conversationModel = ConversationService.Conversations.FirstOrDefault(c => c.Id == conversation.Id) ??
                                    await ConversationService.AddConversation(conversation.Participants.ToArray());

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

                conversationModel.Call = new Call
                {
                    Id = call.Id,
                    Participants = call.Participants.Select(p => UserService.Users.FirstOrDefault(u => u.Id == p)).ToList(),
                    Conversation = conversationModel
                };

                view?.ShowCall(conversationModel.Call);

                CallService.CurrentCall = conversationModel.Call;
                SetCallEvents(call);

                Recorder.Start();
                Player.Start();
            });
        }

        private void ConversationOnParticipantConnected(ClientApi.Conversation conversation, Guid guid)
        {
            
        }

        private void ConversationOnParticipantDisconnected(ClientApi.Conversation conversation, Guid guid)
        {
            
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
            
        }

        private void CallOnParticipantLeft(ClientApi.Call call, Guid guid)
        {
            
        }

        private void CallOnParticipantDeclined(ClientApi.Call call, Guid guid)
        {
            
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

        #region Events

        

        #endregion

        
    }
}
