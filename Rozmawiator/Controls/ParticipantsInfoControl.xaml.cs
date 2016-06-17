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
using Rozmawiator.Models;

namespace Rozmawiator.Controls
{
    /// <summary>
    /// Interaction logic for UserInfoControl.xaml
    /// </summary>
    public partial class ParticipantsInfoControl : UserControl
    {
        private Conversation _conversation;

        public Conversation Conversation
        {
            get { return _conversation; }
            set
            {
                _conversation = value;
                Update();
            }
        }

        public event Action<ParticipantsInfoControl> Called;

        public ParticipantsInfoControl()
        {
            InitializeComponent();
        }

        public ParticipantsInfoControl(Conversation conversation)
        {
            InitializeComponent();
            Conversation = conversation;
        }

        public void Update()
        {
            var participants = _conversation.Participants.Where(p => p.Id != UserService.LoggedUser.Id).ToArray();
            if (!participants.Any())
            {
                return;
            }
            
            ParticipantsNicknames.Content = participants.Select(p => p.Nickname).Aggregate((a,b) => a + ", " + b);
            Avatar.Source = participants.FirstOrDefault(p => p.Avatar != null)?.Avatar ?? Resources["DefaultAvatar"] as ImageSource;
        }

        private void ImageButton_Click(ImageButton arg1, RoutedEventArgs arg2)
        {
            Called?.Invoke(this);
        }
    }
}
