using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Database.Entities
{
    public enum ConversationType
    {
        Passive,
        Active
    }

    public class Conversation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public ConversationType Type { get; set; }
        public Guid? OwnerId { get; set; }
        public Guid? CreatorId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; }
        [ForeignKey("CreatorId")]
        public virtual User Creator { get; set; }
        public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; }
    }
}
