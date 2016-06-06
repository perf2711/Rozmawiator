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
using Rozmawiator.Models;

namespace Rozmawiator.Controls
{
    /// <summary>
    /// Interaction logic for UserThumbnailControl.xaml
    /// </summary>
    public partial class UserThumbnailControl : UserControl
    {
        public enum CallState
        {
            Calling,
            Ongoing,
            Disconnected
        }

        private User _user;
        private CallState _state = CallState.Calling;

        public CallState State
        {
            get { return _state; }
            set
            {
                _state = value;
                UpdateState();
            }
        }

        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                Update();
            }
        }

        public UserThumbnailControl()
        {
            InitializeComponent();
        }

        public UserThumbnailControl(User user)
        {
            InitializeComponent();
            User = user;
        }

        public void Update()
        {
            Avatar.Source = _user.Avatar ?? Resources["DefaultAvatar"] as ImageSource;
            UsernameLabel.Content = _user.Nickname;
        }

        public void UpdateState()
        {
            switch (State)
            {
                case CallState.Calling:
                    AvatarDim.Opacity = 0.8;
                    break;
                case CallState.Ongoing:
                    AvatarDim.Opacity = 0;
                    break;
                case CallState.Disconnected:
                    AvatarDim.Opacity = 0.8;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
