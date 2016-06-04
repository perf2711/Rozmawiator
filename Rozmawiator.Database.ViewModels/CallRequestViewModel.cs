using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.Entities;

namespace Rozmawiator.Database.ViewModels
{
    public class CallRequestViewModel
    {
        public Guid Id { get; set; }
        public CallRequestState State { get; set; }
        public string Caller { get; set; }
        public string Callee { get; set; }
    }
}
