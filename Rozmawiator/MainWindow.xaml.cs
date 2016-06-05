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
using Rozmawiator.Audio;
using Rozmawiator.Controls;
using Rozmawiator.Data;
using Rozmawiator.Extensions;

namespace Rozmawiator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task UpdateData()
        {
            this.ShowLoading("Aktualizowanie danych...");
            await UserService.UpdateLoggedUser();
            await PassiveConversationService.UpdateConversations();

            Dispatcher.Invoke(UpdateViews);

            this.HideLoading();
        }

        private void UpdateViews()
        {
            foreach (var conversation in PassiveConversationService.Conversations)
            {
                var control = new PassiveConversationControl
                {
                    Conversation = conversation
                };
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Task(async () => await UpdateData()).Start();
        }
    }
}
