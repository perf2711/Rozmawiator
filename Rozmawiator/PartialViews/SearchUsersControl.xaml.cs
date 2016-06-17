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
using Rozmawiator.Controls;
using Rozmawiator.Data;
using Rozmawiator.Models;

namespace Rozmawiator.PartialViews
{
    /// <summary>
    /// Interaction logic for SearchUsersControl.xaml
    /// </summary>
    public partial class SearchUsersControl : UserControl
    {
        public event Action<SearchUsersControl, User> AddToNewConversation;
        public event Action<SearchUsersControl, User> AddToConversation;

        public SearchUsersControl()
        {
            InitializeComponent();
        }

        public void Search()
        {
            var query = SearchBox.Text;

            new Task(async () =>
            {
                var results = await UserService.Search(query);

                Dispatcher.Invoke(() => DisplayResults(results));

            }).Start();
        }

        private void DisplayResults(IEnumerable<User> users)
        {
            ResultPanel.Children.Clear();
            foreach (var user in users)
            {
                var control = new UserInfoControl(user);
                control.Add += OnResultAdd;
                control.AddExisting += OnResultAddExisting;
                ResultPanel.Children.Add(control);
            }
        }

        private void OnResultAddExisting(UserInfoControl userInfoControl)
        {
            AddToConversation?.Invoke(this, userInfoControl.User);
        }

        private void OnResultAdd(UserInfoControl userInfoControl)
        {
            AddToNewConversation?.Invoke(this, userInfoControl.User);
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Search();
            }
        }
    }
}
