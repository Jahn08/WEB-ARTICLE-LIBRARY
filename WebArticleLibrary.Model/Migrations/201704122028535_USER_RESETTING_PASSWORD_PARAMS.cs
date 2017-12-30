namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class USER_RESETTING_PASSWORD_PARAMS : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.USER", "RESET_PASSWORD_EXPIRATION_DATE", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.USER", "RESET_PASSWORD_EXPIRATION_DATE");
        }
    }
}
