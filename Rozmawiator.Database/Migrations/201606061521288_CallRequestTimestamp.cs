namespace Rozmawiator.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CallRequestTimestamp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CallRequests", "Timestamp", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CallRequests", "Timestamp");
        }
    }
}
