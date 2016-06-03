using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Server.Api;

namespace Rozmawiator.Server.ViewModels
{
    public class ClientViewModel
    {
        public Client Client { get; set; }

        public int Id => Client.Id;
        public string Nickname => Client.Nickname;
        public string Endpoint => Client.EndPoint.ToString();
    }
}
