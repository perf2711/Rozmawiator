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
    /// Interaction logic for CallControl.xaml
    /// </summary>
    public partial class CallRequestControl : UserControl
    {
        private CallRequest _callRequest;

        public event Action<CallRequestControl> Accepted;
        public event Action<CallRequestControl> Denied;
        public event Action<CallRequestControl> Ignored;

        public CallRequest CallRequest
        {
            get { return _callRequest; }
            set
            {
                _callRequest = value;
                Update();
            }
        }

        public CallRequestControl(CallRequest callRequest)
        {
            InitializeComponent();
            CallRequest = callRequest;
        }

        public void Update()
        {
            CallerLabel.Text = _callRequest.Conversation.Participants.Select(p => p.Nickname).Aggregate((a, b) => a + ", " + b);
        }

        private void AcceptClick(object sender, RoutedEventArgs e)
        {
            Accepted?.Invoke(this);   
        }

        private void DeclineClick(object sender, RoutedEventArgs e)
        {
            Denied?.Invoke(this);
        }

        private void IgnoreClick(object sender, RoutedEventArgs e)
        {
            Ignored?.Invoke(this);
        }
    }
}
