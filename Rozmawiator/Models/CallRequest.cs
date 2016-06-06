using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rozmawiator.Database.Entities;

namespace Rozmawiator.Models
{
    public class CallRequest
    {
        public Guid Id { get; set; }
        public CallRequestState State { get; set; }
        public string Caller { get; set; }
        public short CallerId { get; set; }
        public string Callee { get; set; }
    }
}
