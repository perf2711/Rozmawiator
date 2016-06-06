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
using Rozmawiator.Data;
using Rozmawiator.Database.Entities;
using Conversation = Rozmawiator.Models.Conversation;

namespace Rozmawiator.Controls
{
    /// <summary>
    /// Interaction logic for ConversationControl.xaml
    /// </summary>
    public partial class ConversationControl : UserControl
    {
        private Conversation _conversation;

        public Conversation Conversation
        {
            get { return _conversation; }
            set
            {
                _conversation = value;
                SetLayout();
            }
        }

        public ConversationControl()
        {
            InitializeComponent();
        }

        public void Notify()
        {
            Dispatcher.Invoke(() =>
            {
                Background = new SolidColorBrush(Colors.Orange);
            });
        }

        public void Unnotify()
        {
            Dispatcher.Invoke(() =>
            {
                Background = _conversation.Type == ConversationType.Active ? new SolidColorBrush(Colors.DodgerBlue) : null;
            });
        }

        private void SetLayout()
        {
            var users = _conversation.Participants.Where(u => u.Nickname != UserService.LoggedUser.Nickname).ToArray();
            if (!users.Any())
            {
                return;
            }

            if (users.Length == 1)
            {
                var user = users.First();
                Icon.Source = user.Avatar ?? Resources["DefaultAvatar"] as ImageSource;
                Participants.Content = user.Nickname;
                return;
            }

            Participants.Content = users.Select(u => u.Nickname).Aggregate((a, b) => a + ", " + b);
        }
    }
}
