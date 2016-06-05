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
            LoadingControl.Visibility = Visibility.Visible;

            LoginUsernameBox.IsEnabled = false;
            LoginPasswordBox.IsEnabled = false;
            LoginButton.IsEnabled = false;

            var username = LoginUsernameBox.Text;
            var password = LoginPasswordBox.Password;

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

                        var error = response.GetModel();
                        var errorDescription = error["error_description"] as string ?? "Nieznany błąd.";
                        this.ShowError("Uwaga", errorDescription);
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
