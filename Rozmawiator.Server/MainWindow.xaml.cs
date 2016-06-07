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
using Rozmawiator.Communication;
using Rozmawiator.Communication.Call;
using Rozmawiator.Communication.Conversation;
using Rozmawiator.Communication.Server;
using Rozmawiator.Server.Api;
using Rozmawiator.Server.ViewModels;
using Rozmawiator.Server.Windows;


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
            Dispatcher.Invoke(() =>
            {
                ConversationListView.UpdateLayout();
            });
        }

        private void OnDebugMessage(DateTime dateTime, string s)
        {
            Log(s);
        }

        private void OnNewMessage(IPEndPoint ipEndPoint, Message message)
        {
            switch (message.Category)
            {
                case MessageCategory.Server:
                    HandleServerMessage(ipEndPoint, (ServerMessage)message);
                    break;
                case MessageCategory.Conversation:
                    HandleConversationMessage(ipEndPoint, (ConversationMessage)message);
                    break;
                case MessageCategory.Call:
                    HandleCallMessage(ipEndPoint, (CallMessage)message);
                    break;
            }
        }

        private void HandleServerMessage(IPEndPoint ipEndPoint, ServerMessage message)
        {
            switch (message.Type)
            {
                case ServerMessageType.Hello:
                    LogMessage(ipEndPoint, message, false);
                    break;
                case ServerMessageType.Ok:
                    LogMessage(ipEndPoint, message, true, true);
                    break;
                case ServerMessageType.Bye:
                case ServerMessageType.CreateConversation:
                case ServerMessageType.Error:
                    LogMessage(ipEndPoint, message);
                    break;
                case ServerMessageType.KeepAlive:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleConversationMessage(IPEndPoint ipEndPoint, ConversationMessage message)
        {
            switch (message.Type)
            {
                case ConversationMessageType.AddUser:
                case ConversationMessageType.NewUser:
                case ConversationMessageType.UserLeft:
                    LogMessage(ipEndPoint, message, true, true);
                    break;
                case ConversationMessageType.Bye:
                case ConversationMessageType.Text:
                case ConversationMessageType.CallResponse:
                    LogMessage(ipEndPoint, message);
                    break;
                case ConversationMessageType.CreateCall:
                case ConversationMessageType.CallRequest:
                    LogMessage(ipEndPoint, message, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleCallMessage(IPEndPoint ipEndPoint, CallMessage message)
        {
            switch (message.Type)
            {
                case CallMessageType.NewUser:
                case CallMessageType.UserDeclined:
                case CallMessageType.UserLeft:
                    LogMessage(ipEndPoint, message, true, true);
                    break;
                case CallMessageType.Bye:
                    LogMessage(ipEndPoint, message);
                    break;
                case CallMessageType.Audio:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LogMessage(IPEndPoint endpoint, ServerMessage message, bool appendContent = true, bool guidContent = false)
        {
            Log($"{message.SenderId} [{endpoint}] <{message.Type}> " + (appendContent ? (guidContent ? message.GetGuidContent().ToString() : message.GetStringContent()) : ""));
        }

        private void LogMessage(IPEndPoint endpoint, ConversationMessage message, bool appendContent = true, bool guidContent = false)
        {
            Log($"{message.SenderId} [{endpoint}] <{message.Type}> ConvID:{message.GetConversationId()} : " + (appendContent ? (guidContent ? message.GetGuidContent().ToString() : message.GetStringContent()) : ""));
        }

        private void LogMessage(IPEndPoint endpoint, CallMessage message, bool appendContent = true, bool guidContent = false)
        {
            Log($"{message.SenderId} [{endpoint}] <{message.Type}> CallID:{message.GetCallId()} : " + (appendContent ? (guidContent ? message.GetGuidContent().ToString() : message.GetStringContent()) : ""));
        }

        private void LogSelfMessage(Message message, bool appendContent = true)
        {
            Log($"Server <{message.MessageType}> " + (appendContent ? message.GetStringContent() : ""));
        }

        private void Log(string text, bool appendDate = true)
        {
            Dispatcher.Invoke(() => { ConsoleBox.Text += (appendDate ? $"[{DateTime.Now.ToString("T")}]" : "") + $" {text}\n"; });
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
            /*
            var message = 
            ConsoleSendBox.Text = "";
            SendMessage(message);
            LogSelfMessage(message);
            */
        }

        private void ConsoleSendBox_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.Key == Key.Return)
            {
                var message = new Message(Message.MessageType.Text, ConsoleSendBox.Text);
                ConsoleSendBox.Text = "";
                SendMessage(message);
                LogSelfMessage(message);
            }
            */
        }
    }
}
