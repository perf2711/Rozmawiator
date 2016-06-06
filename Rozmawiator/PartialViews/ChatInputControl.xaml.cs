using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rozmawiator.Data;
using Rozmawiator.Models;

namespace Rozmawiator.PartialViews
{
    /// <summary>
    /// Interaction logic for ChatInputControl.xaml
    /// </summary>
    public partial class ChatInputControl : UserControl
    {
        public Brush UnfocusedForeground
        {
            get { return (Brush) Resources["Unfocused"]; }
            set { Resources["Unfocused"] = value; }
        }

        public Brush FocusedForeground
        {
            get { return (Brush)Resources["Focused"]; }
            set { Resources["Focused"] = value; }
        }

        public event Action<ChatInputControl, TextMessage> Sent;

        public ChatInputControl()
        {
            InitializeComponent();
        }

        public void Send()
        {
            var message = new TextMessage(InputBox.Text, DateTime.Now, UserService.LoggedUser);
            Sent?.Invoke(this, message);
            InputBox.Text = "";
        }

        private void InputBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBorder.BorderBrush = FocusedForeground;
        }

        private void InputBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBorder.BorderBrush = UnfocusedForeground;
        }

        private void InputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Send();
            }
        }
    }
}
