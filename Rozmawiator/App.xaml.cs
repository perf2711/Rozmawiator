using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Rozmawiator.Data;
using Rozmawiator.Models;

namespace Rozmawiator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            ClientService.Client.Disconnect();
        }
    }
}
