using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Rozmawiator.Database.Identity
{
    public class GuidRole : IdentityRole<Guid, GuidUserRole>
    {
    }
}
