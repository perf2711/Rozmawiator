namespace Rozmawiator.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CallRequests",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        ConversationId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Conversations", t => t.ConversationId, cascadeDelete: true)
                .Index(t => t.ConversationId);
            
            CreateTable(
                "dbo.Conversations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Calls",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        ConversationId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Conversations", t => t.ConversationId, cascadeDelete: true)
                .Index(t => t.ConversationId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AvatarPath = c.String(),
                        RegistrationDateTime = c.DateTime(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Content = c.String(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        SenderId = c.Guid(nullable: false),
                        ConversationId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Conversations", t => t.ConversationId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.SenderId, cascadeDelete: true)
                .Index(t => t.SenderId)
                .Index(t => t.ConversationId);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserId = c.Guid(nullable: false),
                        RoleId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.Servers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IpAddress = c.String(nullable: false),
                        Port = c.Int(nullable: false),
                        State = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CallParticipants",
                c => new
                    {
                        UserId = c.Guid(nullable: false),
                        ConversationId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.ConversationId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Calls", t => t.ConversationId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ConversationId);
            
            CreateTable(
                "dbo.ConversationParticipants",
                c => new
                    {
                        UserId = c.Guid(nullable: false),
                        ConversationId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.ConversationId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Conversations", t => t.ConversationId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ConversationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.CallRequests", "ConversationId", "dbo.Conversations");
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.Messages", "SenderId", "dbo.Users");
            DropForeignKey("dbo.Messages", "ConversationId", "dbo.Conversations");
            DropForeignKey("dbo.UserLogins", "UserId", "dbo.Users");
            DropForeignKey("dbo.ConversationParticipants", "ConversationId", "dbo.Conversations");
            DropForeignKey("dbo.ConversationParticipants", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserClaims", "UserId", "dbo.Users");
            DropForeignKey("dbo.CallParticipants", "ConversationId", "dbo.Calls");
            DropForeignKey("dbo.CallParticipants", "UserId", "dbo.Users");
            DropForeignKey("dbo.Calls", "ConversationId", "dbo.Conversations");
            DropIndex("dbo.ConversationParticipants", new[] { "ConversationId" });
            DropIndex("dbo.ConversationParticipants", new[] { "UserId" });
            DropIndex("dbo.CallParticipants", new[] { "ConversationId" });
            DropIndex("dbo.CallParticipants", new[] { "UserId" });
            DropIndex("dbo.Roles", "RoleNameIndex");
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.Messages", new[] { "ConversationId" });
            DropIndex("dbo.Messages", new[] { "SenderId" });
            DropIndex("dbo.UserLogins", new[] { "UserId" });
            DropIndex("dbo.UserClaims", new[] { "UserId" });
            DropIndex("dbo.Users", "UserNameIndex");
            DropIndex("dbo.Calls", new[] { "ConversationId" });
            DropIndex("dbo.CallRequests", new[] { "ConversationId" });
            DropTable("dbo.ConversationParticipants");
            DropTable("dbo.CallParticipants");
            DropTable("dbo.Servers");
            DropTable("dbo.Roles");
            DropTable("dbo.UserRoles");
            DropTable("dbo.Messages");
            DropTable("dbo.UserLogins");
            DropTable("dbo.UserClaims");
            DropTable("dbo.Users");
            DropTable("dbo.Calls");
            DropTable("dbo.Conversations");
            DropTable("dbo.CallRequests");
        }
    }
}
