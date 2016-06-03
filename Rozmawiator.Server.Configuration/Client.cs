using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FX.Configuration;

namespace Rozmawiator.Server.Configuration
{
    public static class Client
    {
        public static int Timeout { get; set; } = 30000;
    }
}
