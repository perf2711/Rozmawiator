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

        private List<User> _temporaryUsers = new List<User>();

        public Call Call
        {
            get { return _call; }
            set
            {
                _call = value;
                Update();
            }
        }

        public CallViewControl()
        {
            InitializeComponent();
        }

        public CallViewControl(Call call)
        {
            InitializeComponent();
            Call = call;
        }

        public void Update()
        {
            if (Call == null)
            {
                return;
            }

            var existingThumbnails = UserPanel.Children.OfType<UserThumbnailControl>().ToList();
            foreach (var participant in _call.Participants)
            {
                if (participant.Id == UserService.LoggedUser.Id || _temporaryUsers.Contains(participant))
                {
                    continue;
                }

                //var userControl = new UserThumbnailControl(participant);
                var userControl = existingThumbnails.FirstOrDefault(t => t.User.Id == participant.Id);
                if (userControl == null)
                {
                    UserPanel.Children.Add(new UserThumbnailControl(participant));
                }
                else
                {
                    existingThumbnails.Remove(userControl);
                }
            }

            foreach (var thumbnail in existingThumbnails)
            {
                if (_temporaryUsers.Contains(thumbnail.User))
                {
                    continue;
                }

                UserPanel.Children.Remove(thumbnail);
            }
        }

        public void AddTemporaryUser(User user, UserThumbnailControl.CallState state)
        {
            _temporaryUsers.Add(user);
            var control = new UserThumbnailControl(user) {State = state};
            UserPanel.Children.Add(control);
        }

        public void RemoveTemporaryUser(User user)
        {
            _temporaryUsers.Remove(user);
            var control = UserPanel.Children.OfType<UserThumbnailControl>().FirstOrDefault(t => t.User.Id == user.Id);
            UserPanel.Children.Remove(control);
        }

        public void ClearTemporaryUsers()
        {
            foreach (var user in _temporaryUsers)
            {
                var control = UserPanel.Children.OfType<UserThumbnailControl>().FirstOrDefault(t => t.User.Id == user.Id);
                UserPanel.Children.Remove(control);
            }
            _temporaryUsers.Clear();
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
