namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ARTICLE_DESCRIPTION_FIELD : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ARTICLE", "DESCRIPTION", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ARTICLE", "DESCRIPTION");
        }
    }
}
