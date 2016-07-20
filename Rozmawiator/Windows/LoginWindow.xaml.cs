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
using System.Windows.Shapes;
using Rozmawiator.ClientApi;
using Rozmawiator.Data;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.Extensions;
using Rozmawiator.RestClient.Models;
using Rozmawiator.Communication;
using Rozmawiator.Communication.Server;

namespace Rozmawiator.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        #region Login

        private void Login()
        {
            Login(LoginUsernameBox.Text, LoginPasswordBox.Password);
        }

        private void Login(string username, string password)
        {
            this.ShowLoading("Logowanie...");

            LoginUsernameBox.IsEnabled = false;
            LoginPasswordBox.IsEnabled = false;
            LoginButton.IsEnabled = false;

            new Task(async () =>
            {
                var response = await RestService.UserApi.Login(username, password);

                if (!response.IsSuccessStatusCode)
                {
                    Dispatcher.Invoke(() =>
                    {
                        this.HideLoading();
                        LoginUsernameBox.IsEnabled = true;
                        LoginPasswordBox.IsEnabled = true;
                        LoginButton.IsEnabled = true;

                        this.ShowError("Uwaga", response.Error.ToString());
                    });
                    return;
                }

                var token = response.GetModel<TokenModel>();
                RestService.CurrentToken = token;
                await UserService.UpdateLoggedUser();
                ConnectToServer();

            }).Start();
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Login();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        #endregion

        #region Register

        private void Register()
        {
            this.ShowLoading("Rejestrowanie...");

            RegisterUsernameBox.IsEnabled = false;
            RegisterEmailBox.IsEnabled = false;
            RegisterPasswordBox.IsEnabled = false;
            RegisterConfirmPasswordBox.IsEnabled = false;
            RegisterButton.IsEnabled = false;

            var registerModel = new RegisterViewModel
            {
                UserName = RegisterUsernameBox.Text,
                Email = RegisterEmailBox.Text,
                Password = RegisterPasswordBox.Password,
                ConfirmPassword = RegisterPasswordBox.Password
            };

            new Task(async () =>
            {
                var response = await RestService.UserApi.Register(registerModel);

                if (!response.IsSuccessStatusCode)
                {
                    Dispatcher.Invoke(() =>
                    {
                        this.HideLoading();
                        RegisterUsernameBox.IsEnabled = true;
                        RegisterEmailBox.IsEnabled = true;
                        RegisterPasswordBox.IsEnabled = true;
                        RegisterConfirmPasswordBox.IsEnabled = true;
                        RegisterButton.IsEnabled = true;

                        this.ShowError("Uwaga", response.Error.ToString());
                    });
                    return;
                }
                Dispatcher.Invoke(() =>
                {
                    Login(registerModel.UserName, registerModel.Password);
                });
            }).Start();
        }

        private void Register_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Register();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Register();
        }

        #endregion

        #region Connect

        private void ConnectToServer()
        {
            this.ShowLoading("Łączenie z serwerem...");

            new Task(async () =>
            {
                var server = await ClientService.GetOnlineServer();
                if (server == null)
                {
                    this.HideLoading();
                    this.ShowError("Uwaga", "W tym momencie żaden serwer nie jest dostępny. Spróbuj ponownie później.");

                    Dispatcher.Invoke(() =>
                    {
                        LoginUsernameBox.IsEnabled = true;
                        LoginPasswordBox.IsEnabled = true;
                        LoginButton.IsEnabled = true;
                        RegisterUsernameBox.IsEnabled = true;
                        RegisterEmailBox.IsEnabled = true;
                        RegisterPasswordBox.IsEnabled = true;
                        RegisterConfirmPasswordBox.IsEnabled = true;
                        RegisterButton.IsEnabled = true;
                    });

                    return;
                }

                ClientService.Client.Connected += OnConnected;
                ClientService.Client.ConnectError += OnConnectError;
                ClientService.Client.Id = UserService.LoggedUser.Id;
                ClientService.Client.Connect(server.EndPoint);

            }).Start();
        }

        private void OnConnected(Client client, Message message)
        {
            this.HideLoading();

            Dispatcher.Invoke(() =>
            {
                new MainWindow().Show();
                Close();
            });
        }

        private void OnConnectError(Client client, ServerMessage serverMessage)
        {
            this.HideLoading();

            LoginUsernameBox.IsEnabled = true;
            LoginPasswordBox.IsEnabled = true;
            LoginButton.IsEnabled = true;

            this.ShowError("Uwaga", "Nie można połączyć się z serwerem. Powód: " + serverMessage.GetStringContent());
        }

        #endregion

        public LoginWindow()
        {
            InitializeComponent();
            /* Debug autofill */
            LoginUsernameBox.Text = "tomek";
            LoginPasswordBox.Password = "fajnehaslo38";
        }
    }
}
