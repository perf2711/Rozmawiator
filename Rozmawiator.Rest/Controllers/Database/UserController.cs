using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using Rozmawiator.Database;
using Rozmawiator.Database.Entities;
using Rozmawiator.Database.Identity;
using Rozmawiator.Database.ViewModels;

namespace Rozmawiator.Rest.Controllers.Database
{
    [Authorize]
    [RoutePrefix("api/Users")]
    public class UserController : ApiController
    {
        private readonly RozmawiatorDb _database = new RozmawiatorDb();

        [HttpGet]
        [Route("{username?}")]
        public UserViewModel GetUser(string username = null)
        {
            var userId = User.GetUserId();

            var user = username == null
                ? _database.Users.FirstOrDefault(m => m.Id == userId)
                : _database.Users.FirstOrDefault(m => m.UserName == username);
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

        [HttpGet]
        [Route("Avatar/{username}")]
        public HttpResponseMessage GetAvatar(string username)
        {
            var user = _database.Users.FirstOrDefault(m => m.UserName == username);
            if (user == null)
            {
                return null;
            }

            var path = HostingEnvironment.MapPath("~/App_Data/Avatars/" + user.Id + ".png");
            if (!File.Exists(path))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            var file = File.ReadAllBytes(path);

            var result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(file)};
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return result;
        }

        [HttpPost]
        [Route("Avatar")]
        public IHttpActionResult SetAvatar()//HttpPostedFileBase imageFile)
        {
            var userId = User.GetUserId();
            var user = _database.Users.FirstOrDefault(m => m.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count <= 0)
            {
                return BadRequest();
            }

            var file = httpRequest.Files.OfType<string>().FirstOrDefault();
            var postedFile = httpRequest.Files[file];
            if (postedFile == null)
            {
                return BadRequest();
            }
            var filePath = HttpContext.Current.Server.MapPath(Path.Combine("~/App_Data/Avatars", userId + Path.GetExtension(postedFile.FileName)));
            postedFile.SaveAs(filePath);

            return Ok();
        }
    }
}
