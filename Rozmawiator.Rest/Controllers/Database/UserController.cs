using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rozmawiator.Database;
using Rozmawiator.Database.Identity;
using Rozmawiator.Database.ViewModels;

namespace Rozmawiator.Rest.Controllers.Database
{
    public class UserController : ApiController
    {
        private readonly RozmawiatorDb _database = new RozmawiatorDb();

        [HttpGet]
        [Route("{id}")]
        public UserViewModel GetUser(string username)
        {
            var user = _database.Users.FirstOrDefault(m => m.UserName == username);
            if (user == null)
            {
                return null;
            }

            return new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                RegistrationDateTime = user.RegistrationDateTime
            };
        }

        [HttpPost]
        [Route("List")]
        public IEnumerable<UserViewModel> GetUsers(Dictionary<string, object> filters)
        {
            return
                new Filter(filters).FilterQuery(
                    _database.Users)
                    .ToArray()
                    .Select(user => new UserViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        RegistrationDateTime = user.RegistrationDateTime
                    });
        }
    }
}
