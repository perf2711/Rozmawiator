using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Rozmawiator.Server.Api;

namespace Rozmawiator.Server
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Listener Server { get; set; }

        public static bool StartServer()
        {
            if (Server.State == Listener.ListenerState.Listening)
            {
                return false;
            }

            Server.Start();
            return true;
        }

        public static bool StopServer()
        {
            if (Server.State == Listener.ListenerState.Idle)
            {
                return false;
            }

            Server.Stop();
            return true;
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
        }
    }
}
