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

        public Guid Id => Client.Id;
        public string Endpoint => Client.EndPoint.ToString();
    }
}
