using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Database.Identity
{
    public static class IdentityExtensions
    {
        public static Guid GetUserId(this IIdentity identity)
        {
            Guid result;
            var id = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(identity);
            Guid.TryParse(id, out result);
            return result;
        }

        public static Guid GetUserId(this IPrincipal principal)
        {
            var identity = principal.Identity;
            return identity.GetUserId();
        }

        public static string GetUserName(this IIdentity identity)
        {
            return Microsoft.AspNet.Identity.IdentityExtensions.GetUserName(identity);
        }

        public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
        {
            return Microsoft.AspNet.Identity.IdentityExtensions.FindFirstValue(identity, claimType);
        }
    }
}
