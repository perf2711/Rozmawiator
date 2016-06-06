using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.ClientApi;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.Models;
using Rozmawiator.RestClient.Errors;

namespace Rozmawiator.Data
{
    public static class ClientService
    {
        public static Client Client { get; } = new Client();

        public static async Task<Server> GetOnlineServer()
        {
            var response = await RestService.ServerApi.GetOnline(RestService.CurrentToken);
            if (response.Error != null)
            {
                if (response.ResponseCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw new RestErrorException(response.Error);
            }

            var serverViewModel = response.GetModel<ServerViewModel>();
            if (serverViewModel == null)
            {
                return null;
            }

            return new Server
            {
                EndPoint = new IPEndPoint(IPAddress.Parse(serverViewModel.IpAddress), serverViewModel.Port),
                State = serverViewModel.State
            };
        }
    }
}
