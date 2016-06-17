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

namespace Rozmawiator.Controls
{
    /// <summary>
    /// Interaction logic for UserInfoControl.xaml
    /// </summary>
    public partial class UserInfoControl : UserControl
    {
        private User _user;

        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                Update();
            }
        }

        public event Action<UserInfoControl> Add;
        public event Action<UserInfoControl> AddExisting;

        public UserInfoControl(User user)
        {
            InitializeComponent();
            User = user;
        }

        public void Update()
        {
            Icon.Source = _user.Avatar ?? Resources["DefaultAvatar"] as ImageSource;
            Nickname.Content = _user.Nickname;
        }

        private void OnAddClick(ImageButton arg1, RoutedEventArgs arg2)
        {
            Add?.Invoke(this);
        }

        private void OnAddExistingClick(ImageButton arg1, RoutedEventArgs arg2)
        {
            AddExisting?.Invoke(this);
        }
    }
}
