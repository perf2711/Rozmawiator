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
    /// Interaction logic for MessageControl.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        private ChatMessage _message;
        private bool _senderInfoVisibility = true;

        public ChatMessage Message
        {
            get { return _message; }
            set
            {
                _message = value;
                if (_message.Sender != null)
                {
                    SenderNickname.Content = _message.Sender.Nickname;
                    SenderAvatar.Source = _message.Sender.Avatar ?? Resources["DefaultAvatar"] as ImageSource;

                    SenderAvatar.Visibility = Visibility.Visible;
                    Bubble.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else
                {
                    SenderInfoVisibility = false;
                    SenderAvatar.Source = null;

                    Bubble.HorizontalAlignment = HorizontalAlignment.Right;
                }
                
                Bubble.Content = _message.Content;
                TimeStamp.Content = _message.Timestamp.ToString("T");
            }
        }

        public bool SenderInfoVisibility
        {
            get { return _senderInfoVisibility; }
            set
            {
                if (_message.Sender == null)
                {
                    return;
                }

                _senderInfoVisibility = value;
                SetSenderInfoVisibility();
            }
        }

        public MessageControl()
        {
            InitializeComponent();
        }

        private void SetSenderInfoVisibility()
        {
            if (SenderInfoVisibility)
            {
                SenderAvatar.Visibility = Visibility.Visible;
                SenderNickname.Visibility = Visibility.Visible;
            }
            else
            {
                SenderAvatar.Visibility = Visibility.Hidden;
                SenderNickname.Visibility = Visibility.Collapsed;
            }
        }
    }
}
