namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class USER_CONFIRMATION_PARAMS : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.USER");
            AddColumn("dbo.USER", "INSERT_DATE", c => c.DateTime(nullable: false));
            AddColumn("dbo.USER", "EXPIRATION_DATE", c => c.DateTime());
            AddColumn("dbo.USER", "CONFIRMATION_ID", c => c.Guid());
            AlterColumn("dbo.USER", "ID", c => c.Int(nullable: false));
            AlterColumn("dbo.USER", "HASH", c => c.String(nullable: false, maxLength: 250));
            AddPrimaryKey("dbo.USER", "ID");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.USER");
            AlterColumn("dbo.USER", "HASH", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.USER", "ID", c => c.Int(nullable: false));
            DropColumn("dbo.USER", "CONFIRMATION_ID");
            DropColumn("dbo.USER", "EXPIRATION_DATE");
            DropColumn("dbo.USER", "INSERT_DATE");
            AddPrimaryKey("dbo.USER", "ID");
        }
    }
}
