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
using Rozmawiator.Data;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.Extensions;
using Rozmawiator.RestClient.Models;

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
            LoadingControl.Text = "Logowanie...";
            LoadingControl.Visibility = Visibility.Visible;

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
                        LoadingControl.Visibility = Visibility.Collapsed;
                        LoginUsernameBox.IsEnabled = true;
                        LoginPasswordBox.IsEnabled = true;
                        LoginButton.IsEnabled = true;

                        this.ShowError("Uwaga", response.Error.ToString());
                    });
                    return;
                }

                var token = response.GetModel<TokenModel>();
                RestService.CurrentToken = token;

                Dispatcher.Invoke(() =>
                {
                    new MainWindow().Show();
                    Close();
                });

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
            LoadingControl.Text = "Rejestrowanie...";
            LoadingControl.Visibility = Visibility.Visible;

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
                        LoadingControl.Visibility = Visibility.Collapsed;
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

        public LoginWindow()
        {
            InitializeComponent();
        }

        
    }
}
