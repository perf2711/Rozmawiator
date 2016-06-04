using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Rozmawiator.Database.Identity;

namespace Rozmawiator.Database.Entities
{
    public class User : IdentityUser<Guid, GuidUserLogin, GuidUserRole, GuidUserClaim>
    {
        public User()
        {
            RegistrationDateTime = DateTime.Now;
        }

        public string AvatarPath { get; set; }
        [Required]
        public DateTime RegistrationDateTime { get; set; }
        
        public virtual ICollection<User> Friends { get; set; }
        public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; }
        public virtual ICollection<Conversation> OwnerConversations { get; set; }
        public virtual ICollection<Conversation> CreatedConversations { get; set; }
        public virtual ICollection<CallRequest> CallerCallRequests { get; set; }
        public virtual ICollection<CallRequest> CalleeCallRequests { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<FriendRequest> RequestedFriendRequests { get; set; }
        public virtual ICollection<FriendRequest> FriendRequests { get; set; }
    }
}
