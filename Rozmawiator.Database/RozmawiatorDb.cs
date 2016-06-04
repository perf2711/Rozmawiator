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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            /* Friends mapping */
            modelBuilder
                .Entity<User>()
                .HasMany(u => u.Friends)
                .WithMany(u => u.Friends)
                .Map(m =>
                {
                    m.ToTable("FriendBinds");
                    m.MapLeftKey("User1Id");
                    m.MapRightKey("User2Id");
                });

            /* Caller and callee mapping */
            modelBuilder
                .Entity<User>()
                .HasMany(u => u.CallerCallRequests)
                .WithRequired(u => u.Caller);
            modelBuilder
                .Entity<User>()
                .HasMany(u => u.CalleeCallRequests)
                .WithRequired(u => u.Callee);

            base.OnModelCreating(modelBuilder);

            /* Rename Identity tables */
            modelBuilder.Entity<User>().ToTable("Users", "dbo");
            modelBuilder.Entity<GuidRole>().ToTable("Roles", "dbo");
            modelBuilder.Entity<GuidUserRole>().ToTable("UserRoles", "dbo");
            modelBuilder.Entity<GuidUserClaim>().ToTable("UserClaims", "dbo");
            modelBuilder.Entity<GuidUserLogin>().ToTable("UserLogins", "dbo");
        }

        public DbSet<CallRequest> CallRequests { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Server> Servers { get; set; }
    }
}
