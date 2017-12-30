namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AMENDMENT_ARCHIVED_FIELD : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AMENDMENT", "ARCHIVED", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AMENDMENT", "ARCHIVED");
        }
    }
}
