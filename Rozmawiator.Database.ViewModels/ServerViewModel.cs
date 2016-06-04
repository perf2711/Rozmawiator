using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.Entities;

namespace Rozmawiator.Database.ViewModels
{
    public class ServerViewModel
    {
        public Guid Id { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public ServerState State { get; set; }
    }
}
