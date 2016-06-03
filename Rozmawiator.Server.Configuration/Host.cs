using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FX.Configuration;

namespace Rozmawiator.Server.Configuration
{
    public static class Host
    {
        public static string Name { get; set; } = "Server";
        public static int ListenPort { get; set; } = 1234;
        public static int MaxClients { get; set; } = 100;
        public static string Motd { get; set; } = "Message of the day";
    }
}
