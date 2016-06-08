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
using Rozmawiator.Controls;
using Rozmawiator.Data;
using Rozmawiator.Models;

namespace Rozmawiator.PartialViews
{
    /// <summary>
    /// Interaction logic for ConversationViewControl.xaml
    /// </summary>
    public partial class ConversationViewControl : UserControl
    {
        public Conversation Conversation { get; }
        public Call Call { get; set; }
        public MessageDisplayControl MessageDisplay { get; private set; }

        public event Action<ConversationViewControl> Called;

        public event Action<ConversationViewControl, bool> MicrophoneToggled;
        public event Action<ConversationViewControl, bool> SpeakerToggled;
        public event Action<ConversationViewControl> HangedUp;

        public event Action<ConversationViewControl, TextMessage> Sent;

        public ConversationViewControl(Conversation conversation)
        {
            InitializeComponent();
            Conversation = conversation;
            SetEvents();
            Update();
        }

        public void Update()
        {
            ParticipantsInfo.Conversation = Conversation;

            if (MessagesPanel.Children.Count == 0)
            {
                MessageDisplay = new MessageDisplayControl(Conversation);
                MessagesPanel.Children.Add(MessageDisplay);
            }

            var conversation = ClientService.Client.Conversations.FirstOrDefault(c => c.Id == Conversation.Id);
            if (conversation?.Call == null)
            {
                CallGrid.Visibility = Visibility.Collapsed;
                CallSplitter.Visibility = Visibility.Collapsed;
            }
        }

        public void AddMessage(TextMessage message)
        {
            MessageDisplay.AddMessageControl(message);
        }

        public void ShowCall(Call call)
        {
            Call = call;
            CallView.Call = call;

            CallSplitter.Visibility = Visibility.Visible;
            CallGrid.Visibility = Visibility.Visible;
        }

        public void HideCall()
        {
            CallView.Call = null;
            Call = null;

            CallGrid.Visibility = Visibility.Collapsed;
            CallSplitter.Visibility = Visibility.Collapsed;
        }

        private void SetEvents()
        {
            ParticipantsInfo.Called += ParticipantsInfoOnCalled;

            CallView.MicrophoneToggled += CallViewOnMicrophoneToggled;
            CallView.SpeakerToggled += CallViewOnSpeakerToggled;
            CallView.HangedUp += CallViewOnHangedUp;

            ChatInput.Sent += ChatInputOnSent;
        }

        private void ParticipantsInfoOnCalled(ParticipantsInfoControl participantsInfoControl)
        {
            Called?.Invoke(this);
        }

        private void CallViewOnMicrophoneToggled(CallViewControl callViewControl, bool b)
        {
            MicrophoneToggled?.Invoke(this, b);
        }

        private void CallViewOnSpeakerToggled(CallViewControl callViewControl, bool b)
        {
            SpeakerToggled?.Invoke(this, b);
        }

        private void CallViewOnHangedUp(CallViewControl callViewControl)
        {
            HangedUp?.Invoke(this);
        }

        private void ChatInputOnSent(ChatInputControl chatInputControl, TextMessage textMessage)
        {
            Sent?.Invoke(this, textMessage);
        }
    }
}
