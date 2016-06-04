using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.ClientApi;

namespace Rozmawiator.Data
{
    public static class ClientService
    {
        public static Client Client { get; } = new Client();
    }
}
