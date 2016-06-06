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
    /// Interaction logic for ActiveConversationViewControl.xaml
    /// </summary>
    public partial class ActiveConversationViewControl : UserControl
    {
        private Conversation _conversation;

        public event Action<ActiveConversationViewControl, bool> MicrophoneToggled;
        public event Action<ActiveConversationViewControl, bool> SpeakerToggled;
        public event Action<ActiveConversationViewControl> HangedUp;

        public Conversation Conversation
        {
            get { return _conversation; }
            set
            {
                _conversation = value;
                Update();
            }
        }

        public ActiveConversationViewControl(Conversation conversation)
        {
            InitializeComponent();
            Conversation = conversation;
        }

        public void Update()
        {
            UserPanel.Children.Clear();

            foreach (var participant in _conversation.Participants)
            {
                if (participant.Nickname == UserService.LoggedUser.Nickname)
                {
                    continue;
                }

                var userControl = new UserThumbnailControl(participant);
                UserPanel.Children.Add(userControl);
            }

            foreach (var calledClient in ClientService.Client.Conversation.CalledClients)
            {
                var user = UserService.GetUser(calledClient);
                var userControl = new UserThumbnailControl(user);
                UserPanel.Children.Add(userControl);
            }
        }

        private void MicrophoneStateButton_Click(ImageButton button, RoutedEventArgs arg2)
        {
            MicrophoneToggled?.Invoke(this, MicrophoneStateButton.State);
        }

        private void SpeakerStateButton_Click(ImageButton button, RoutedEventArgs arg2)
        {
            SpeakerToggled?.Invoke(this, SpeakerStateButton.State);
        }

        private void DropCallButton_Click(ImageButton button, RoutedEventArgs arg2)
        {
            HangedUp?.Invoke(this);
        }
    }
}
