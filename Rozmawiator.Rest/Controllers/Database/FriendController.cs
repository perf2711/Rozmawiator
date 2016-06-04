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
    [Authorize]
    [RoutePrefix("api/Friends")]
    public class FriendController : ApiController
    {
        private readonly RozmawiatorDb _database = new RozmawiatorDb();

        [HttpPost]
        [Route("List")]
        public IEnumerable<UserViewModel> GetFriends(Dictionary<string, object> filters)
        {
            var userId = User.GetUserId();

            return
                new Filter(filters).FilterQuery(
                    _database.Users.Where(u => u.Friends.Any(f => f.Id == userId)))
                    .ToArray()
                    .Select(user => new UserViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        RegistrationDateTime = user.RegistrationDateTime
                    });
        }

        [HttpGet]
        [Route("{id}")]
        public FriendRequestViewModel GetFriendRequest(Guid id)
        {
            var friendRequest = _database.FriendRequests.FirstOrDefault(fr => fr.Id == id);
            if (friendRequest == null)
            {
                return null;
            }

            return new FriendRequestViewModel
            {
                Id = friendRequest.Id,
                RequestingUser = friendRequest.RequestingUser.UserName,
                TargetUser = friendRequest.TargetUser.UserName
            };
        }

        [HttpPost]
        [Route("List")]
        public IEnumerable<FriendRequestViewModel> GetFriendRequests(Dictionary<string, object> filters)
        {
            var userId = User.GetUserId();

            return
                new Filter(filters).FilterQuery(
                    _database.FriendRequests.Where(fr => fr.RequestingUserId == userId || fr.TargetUserId == userId))
                    .ToArray()
                    .Select(friendRequest => new FriendRequestViewModel
                    {
                        Id = friendRequest.Id,
                        RequestingUser = friendRequest.RequestingUser.UserName,
                        TargetUser = friendRequest.TargetUser.UserName
                    });
        }
    }
}
