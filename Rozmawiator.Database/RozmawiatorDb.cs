using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Rozmawiator.Database.Entities;
using Rozmawiator.Database.Identity;

namespace Rozmawiator.Database
{
    public class RozmawiatorDb : IdentityDbContext<User, GuidRole, Guid, GuidUserLogin, GuidUserRole, GuidUserClaim>
    {
        public RozmawiatorDb() : base("name=RozmawiatorDb")
        {
            
        }

        public static RozmawiatorDb Create()
        {
            return new RozmawiatorDb();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            /* Conversation - User mapping */
            modelBuilder
                .Entity<User>()
                .HasMany(u => u.Conversations)
                .WithMany(c => c.Participants)
                .Map(m =>
                {
                    m.MapLeftKey("UserId");
                    m.MapRightKey("ConversationId");
                    m.ToTable("ConversationParticipants");
                });

            /* Call - User mapping */
            modelBuilder
                .Entity<User>()
                .HasMany(u => u.Calls)
                .WithMany(u => u.Participants)
                .Map(m =>
                {
                    m.MapLeftKey("UserId");
                    m.MapRightKey("ConversationId");
                    m.ToTable("CallParticipants");
                });

            base.OnModelCreating(modelBuilder);

            /* Rename Identity tables */
            modelBuilder.Entity<User>().ToTable("Users", "dbo");
            modelBuilder.Entity<GuidRole>().ToTable("Roles", "dbo");
            modelBuilder.Entity<GuidUserRole>().ToTable("UserRoles", "dbo");
            modelBuilder.Entity<GuidUserClaim>().ToTable("UserClaims", "dbo");
            modelBuilder.Entity<GuidUserLogin>().ToTable("UserLogins", "dbo");
        }

        public DbSet<CallRequest> CallRequests { get; set; }
        //public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Server> Servers { get; set; }
    }
}
