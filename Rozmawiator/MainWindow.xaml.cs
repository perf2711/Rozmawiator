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
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Controls;
using Rozmawiator.Data;
using Rozmawiator.Database.Entities;
using Rozmawiator.Extensions;
using Rozmawiator.Models;
using Rozmawiator.PartialViews;
using Call = Rozmawiator.ClientApi.Call;
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

        private List<MessageDisplayControl> MessageDisplays { get; } = new List<MessageDisplayControl>();
        private MessageDisplayControl CurrentMessageDisplay { get; set; }

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

        private void ConversationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var conversation = SelectedConversation.Conversation;
            var display = MessageDisplays.FirstOrDefault(m => m.Conversation == conversation);
            if (display == null)
            {
                display = new MessageDisplayControl(conversation);
                MessageDisplays.Add(display);
            }
            CurrentMessageDisplay = display;
        }

        private async void OnNewConversation(Client client, ClientApi.Conversation conversation)
        {
            var conversationModel = ConversationService.Conversations.FirstOrDefault(c => c.Id == conversation.Id) ??
                                    await ConversationService.AddConversation(conversation.Participants.ToArray());

            var control = new ConversationControl { Conversation = conversationModel };
            ConversationList.Items.Add(control);
            SetConversationEvents(conversation);
        }

        private void SetConversationEvents(ClientApi.Conversation conversation)
        {
            conversation.NewTextMessage += ConversationOnNewTextMessage;
            conversation.NewCallRequest += ConversationOnNewCallRequest;
            conversation.NewCall += ConversationOnNewCall;
            conversation.ParticipantConnected += ConversationOnParticipantConnected;
            conversation.ParticipantDisconnected += ConversationOnParticipantDisconnected;
        }

        private void ConversationOnNewTextMessage(ClientApi.Conversation conversation, ConversationMessage conversationMessage)
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

            var messageDisplay = MessageDisplays.FirstOrDefault(m => m.Conversation == conversationModel);
            messageDisplay?.AddMessageControl(textMessage);
        }

        private void ConversationOnNewCallRequest(ClientApi.Conversation conversation, ClientApi.CallRequest callRequest)
        {
            throw new NotImplementedException();
        }

        private void ConversationOnNewCall(ClientApi.Conversation conversation, Call call)
        {
            throw new NotImplementedException();
        }

        private void ConversationOnParticipantConnected(ClientApi.Conversation conversation, Guid guid)
        {
            throw new NotImplementedException();
        }

        private void ConversationOnParticipantDisconnected(ClientApi.Conversation conversation, Guid guid)
        {
            throw new NotImplementedException();
        }

        private void MessageInputBox_Sent(ChatInputControl arg1, TextMessage arg2)
        {

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
