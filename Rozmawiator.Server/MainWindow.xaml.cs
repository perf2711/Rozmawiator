using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
using Rozmawiator.Server.Api;
using Rozmawiator.Server.ViewModels;
using Rozmawiator.Server.Windows;
using Rozmawiator.Shared;

namespace Rozmawiator.Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Status
        {
            get { return (string) StatusLabel.GetValue(ContentProperty); }
            set { StatusLabel.SetValue(ContentProperty, value); }
        }

        public MainWindow()
        {
            App.Server = new Listener();

            App.Server.NewMessage += OnNewMessage;
            App.Server.DebugMessage += OnDebugMessage;
            App.Server.ClientConnected += OnClientConnect;
            App.Server.ClientDisconnected += OnClientDisconnect;
            App.Server.ConversationCreated += OnConversationCreate;
            App.Server.ConversationClosed += OnConversationClose;
            App.Server.ConversationUpdate += OnConversationUpdate;

            InitializeComponent();
        }

        private void OnClientConnect(Client client)
        {
            var clientViewModel = new ClientViewModel
            {
                Client = client
            };

            Dispatcher.Invoke(() => ClientListView.Items.Add(clientViewModel));
        }

        private void OnClientDisconnect(Client client)
        {
            var clientViewModel = ClientListView.Items.OfType<ClientViewModel>().FirstOrDefault(cv => cv.Client == client);
            if (clientViewModel != null)
            {
                Dispatcher.Invoke(() => ClientListView.Items.Remove(clientViewModel));
            }
        }

        private void OnConversationCreate(Conversation conversation)
        {
            var conversationViewModel = new ConversationViewModel
            {
                Conversation = conversation
            };

            Dispatcher.Invoke(() => ConversationListView.Items.Add(conversationViewModel));
        }

        private void OnConversationClose(Conversation conversation)
        {
            var conversationViewModel =
                ConversationListView.Items.OfType<ConversationViewModel>().FirstOrDefault(cv => cv.Conversation == conversation);
            if (conversationViewModel != null)
            {
                Dispatcher.Invoke(() => ConversationListView.Items.Remove(conversationViewModel));
            }
        }

        private void OnConversationUpdate(Conversation conversation)
        {
            ConversationListView.UpdateLayout();
        }

        private void OnDebugMessage(DateTime dateTime, string s)
        {
            Log(s);
        }

        private void OnNewMessage(IPEndPoint ipEndPoint, Message message)
        {
            switch (message.Type)
            {
                case Message.MessageType.Hello:
                case Message.MessageType.Bye:
                case Message.MessageType.Call:
                case Message.MessageType.CallRequest:
                case Message.MessageType.Text:
                    LogMessage(ipEndPoint, message);
                    break;
                case Message.MessageType.DirectText:
                    LogDirectText(ipEndPoint, message);
                    break;
                case Message.MessageType.KeepAlive:
                case Message.MessageType.Audio:
                case Message.MessageType.CallResponse:
                case Message.MessageType.HelloConversation:
                case Message.MessageType.ByeConversation:
                case Message.MessageType.CloseConversation:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LogMessage(IPEndPoint endpoint, Message message, bool appendContent = true)
        {
            Log($"{App.Server.GetClient(message.Sender)?.Nickname ?? "?"} [{endpoint}] <{message.Type}> " + (appendContent ? message.GetTextContent() : ""));
        }

        private void LogDirectText(IPEndPoint endpoint, Message message, bool appendContent = true)
        {
            Log($"{App.Server.GetClient(message.Sender)?.Nickname ?? "?"} [{endpoint}] => {message.GetDirectTextNickname()} <{message.Type}> " + (appendContent ? message.GetDirectTextContent() : ""));
        }

        private void LogSelfMessage(Message message, bool appendContent = true)
        {
            Log($"Server <{message.Type}> " + (appendContent ? message.GetTextContent() : ""));
        }

        private void Log(string text, bool appendDate = true)
        {
            Dispatcher.Invoke(() =>
            {
                ConsoleBox.Text += (appendDate ? $"[{DateTime.Now.ToString("T")}]" : "") + $" {text}\n";
            });
        }

        private void SendMessage(Message message)
        {
            App.Server.Broadcast(message);
        }

        private void StartServer()
        {
            if (!App.StartServer())
            {
                return;
            }

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            ConsoleSendButton.IsEnabled = true;
            ConsoleSendBox.IsEnabled = true;
            Resources["Base"] = Colors.DodgerBlue;

            Status = $"Server is listening on port {App.Server.Port}.";
        }

        private void StopServer()
        {
            if (!App.StopServer())
            {
                return;
            }

            ClientListView.Items.Clear();
            ConversationListView.Items.Clear();

            StopButton.IsEnabled = false;
            StartButton.IsEnabled = true;
            ConsoleSendButton.IsEnabled = false;
            ConsoleSendBox.IsEnabled = false;
            Resources["Base"] = Colors.Gray;

            Status = "Server is offline.";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartServer();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopServer();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(typeof(Configuration.Client), typeof(Configuration.Host));
            settings.ShowDialog();
        }

        private void OnClose(object sender, CancelEventArgs e)
        {
            if (App.Server.State != Listener.ListenerState.Listening) return;

            if (MessageBox.Show("Stop server and exit?", "Notice", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                StopServer();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var message = new Message(Message.MessageType.Text, ConsoleSendBox.Text);
            ConsoleSendBox.Text = "";
            SendMessage(message);
            LogSelfMessage(message);
        }

        private void ConsoleSendBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var message = new Message(Message.MessageType.Text, ConsoleSendBox.Text);
                ConsoleSendBox.Text = "";
                SendMessage(message);
                LogSelfMessage(message);
            }
        }
    }
}
