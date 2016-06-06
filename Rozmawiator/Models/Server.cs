using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.Entities;

namespace Rozmawiator.Models
{
    public class Server
    {
        public IPEndPoint EndPoint { get; set; }
        public ServerState State { get; set; }
    }
}
