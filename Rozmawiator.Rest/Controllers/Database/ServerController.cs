using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rozmawiator.Database;
using Rozmawiator.Database.Entities;
using Rozmawiator.Database.ViewModels;

namespace Rozmawiator.Rest.Controllers.Database
{
    [Authorize]
    [RoutePrefix("api/Servers")]
    public class ServerController : ApiController
    {
        private readonly RozmawiatorDb _database = new RozmawiatorDb();

        [HttpGet]
        [Route("{id}")]
        public ServerViewModel GetServer(Guid id)
        {
            var server = _database.Servers.FirstOrDefault(s => s.Id == id);
            if (server == null)
            {
                return null;
            }

            return new ServerViewModel
            {
                Id = server.Id,
                IpAddress = server.IpAddress,
                Port = server.Port,
                State = server.State
            };
        }

        [HttpGet]
        [Route("")]
        public ServerViewModel GetOnlineServer()
        {
            var server =  _database.Servers.FirstOrDefault(s => s.State == ServerState.Online);
            if (server == null)
            {
                return null;
            }

            return new ServerViewModel
            {
                Id = server.Id,
                IpAddress = server.IpAddress,
                Port = server.Port,
                State = server.State
            };
        }

        [HttpPost]
        [Route("List")]
        public IEnumerable<ServerViewModel> GetServers(Dictionary<string, object> filters)
        {
            return new Filter(filters).FilterQuery(_database.Servers).ToArray().Select(server => new ServerViewModel
            {
                Id = server.Id,
                IpAddress = server.IpAddress,
                Port = server.Port,
                State = server.State
            });
        }
    }
}
