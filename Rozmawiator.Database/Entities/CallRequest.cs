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
        public CallRequest()
        {
            Timestamp = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public CallRequestState State { get; set; }
        [Required]
        public Guid CallerId { get; set; }
        [Required]
        public Guid CalleeId { get; set; }
        public DateTime Timestamp { get; set; }

        [ForeignKey("CallerId")]
        public virtual User Caller { get; set; }
        [ForeignKey("CalleeId")]
        public virtual User Callee { get; set; }
    }
}
