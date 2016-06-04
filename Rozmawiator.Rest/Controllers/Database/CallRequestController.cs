using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security;
using Rozmawiator.Database;
using Rozmawiator.Database.Identity;
using Rozmawiator.Database.ViewModels;

namespace Rozmawiator.Rest.Controllers.Database
{
    [Authorize]
    [RoutePrefix("api/CallRequests")]
    public class CallRequestController : ApiController
    {
        private readonly RozmawiatorDb _database = new RozmawiatorDb();

        [HttpGet]
        [Route("{id}")]
        public CallRequestViewModel GetCallRequest(Guid id)
        {
            var callRequest = _database.CallRequests.FirstOrDefault(cr => cr.Id == id);
            if (callRequest == null)
            {
                return null;
            }

            return new CallRequestViewModel
            {
                Id = callRequest.Id,
                State = callRequest.State,
                Caller = callRequest.Caller.UserName,
                Callee = callRequest.Callee.UserName
            };
        }
        
        [HttpPost]
        [Route("List")]
        public IEnumerable<CallRequestViewModel> GetCallRequests(Dictionary<string, object> filters)
        {
            var user = User.GetUserId();

            return new Filter(filters).FilterQuery(_database.CallRequests.Where(cr => cr.CalleeId == user || cr.CallerId == user)).ToArray().Select(callRequest => new CallRequestViewModel
            {
                Id = callRequest.Id,
                State = callRequest.State,
                Caller = callRequest.Caller.UserName,
                Callee = callRequest.Callee.UserName
            });
        }
    }
}
