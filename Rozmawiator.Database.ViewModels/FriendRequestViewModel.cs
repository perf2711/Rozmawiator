using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Database.ViewModels
{
    public class FriendRequestViewModel
    {
        public Guid Id { get; set; }
        public string RequestingUser { get; set; }
        public string TargetUser { get; set; }
    }
}
