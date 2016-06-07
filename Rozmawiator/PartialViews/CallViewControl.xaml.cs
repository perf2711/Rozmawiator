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
    public partial class CallViewControl : UserControl
    {
        private Call _call;

        public event Action<CallViewControl, bool> MicrophoneToggled;
        public event Action<CallViewControl, bool> SpeakerToggled;
        public event Action<CallViewControl> HangedUp;

        public Call Call
        {
            get { return _call; }
            set
            {
                _call = value;
                Update();
            }
        }

        public CallViewControl(Call call)
        {
            InitializeComponent();
            Call = call;
        }

        public void Update()
        {
            UserPanel.Children.Clear();

            foreach (var participant in _call.Participants)
            {
                if (participant.Nickname == UserService.LoggedUser.Nickname)
                {
                    continue;
                }

                var userControl = new UserThumbnailControl(participant);
                UserPanel.Children.Add(userControl);
            }

            foreach (var user in Call.Participants)
            {
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
