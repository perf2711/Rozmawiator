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
using Rozmawiator.ClientApi;
using Rozmawiator.Data;
using Rozmawiator.Extensions;

namespace Rozmawiator.Controls
{
    /// <summary>
    /// Interaction logic for PassiveConversationControl.xaml
    /// </summary>
    public partial class PassiveConversationControl : UserControl
    {
        private PassiveConversation _conversation;

        public PassiveConversation Conversation
        {
            get { return _conversation; }
            set
            {
                _conversation = value;
                SetLayout();
            }
        }

        public PassiveConversationControl()
        {
            InitializeComponent();
        }

        private void SetLayout()
        {
            var users = _conversation.GetUsers().Where(u => u.Nickname != UserService.LoggedUser.Nickname).ToArray();
            if (!users.Any())
            {
                return;
            }

            if (users.Count() == 1)
            {
                var user = users.First();
                if (user.Avatar != null)
                {
                    Icon.Source = user.Avatar;
                }

                Participants.Content = user.Nickname;
            }

            Participants.Content = users.Select(u => u.Nickname).Aggregate((a, b) => a + ", " + b);
        }
    }
}
