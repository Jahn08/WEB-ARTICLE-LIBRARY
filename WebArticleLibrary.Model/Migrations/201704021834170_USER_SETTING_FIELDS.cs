namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class USER_SETTING_FIELDS : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.USER", "NEW_EMAIL", c => c.String());
            AddColumn("dbo.USER", "NEW_EMAIL_EXPIRATION_DATE", c => c.DateTime());
            AddColumn("dbo.USER", "STATUS", c => c.Int(nullable: false));
            AddColumn("dbo.USER", "SHOW_PRIVATE_INFO", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.USER", "SHOW_PRIVATE_INFO");
            DropColumn("dbo.USER", "STATUS");
            DropColumn("dbo.USER", "NEW_EMAIL_EXPIRATION_DATE");
            DropColumn("dbo.USER", "NEW_EMAIL");
        }
    }
}
