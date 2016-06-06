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
using Rozmawiator.Models;

namespace Rozmawiator.PartialViews
{
    /// <summary>
    /// Interaction logic for LoggedUserInfo.xaml
    /// </summary>
    public partial class LoggedUserInfoControl : UserControl
    {
        private User _user;

        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                UsernameLabel.Content = _user.Nickname;
                Avatar.Source = _user.Avatar ?? Resources["DefaultAvatar"] as ImageSource;
            }
        }

        public LoggedUserInfoControl()
        {
            InitializeComponent();
        }
    }
}
