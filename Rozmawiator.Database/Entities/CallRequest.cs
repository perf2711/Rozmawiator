using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Database.Entities
{
    public enum CallRequestState
    {
        RequestDenied,
        RequestAccepted,
        NoResponse,
    }

    public class CallRequest
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public CallRequestState State { get; set; }
        [Required]
        public Guid CallerId { get; set; }
        [Required]
        public Guid CalleeId { get; set; }

        [ForeignKey("CallerId")]
        public virtual User Caller { get; set; }
        [ForeignKey("CalleeId")]
        public virtual User Callee { get; set; }
    }
}
