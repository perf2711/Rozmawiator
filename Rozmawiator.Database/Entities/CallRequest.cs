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
        public DateTime Timestamp { get; set; }
        [Required]
        public Guid ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }
    }
}
