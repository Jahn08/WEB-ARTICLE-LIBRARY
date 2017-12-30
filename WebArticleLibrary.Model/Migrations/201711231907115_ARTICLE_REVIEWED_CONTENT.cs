namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ARTICLE_REVIEWED_CONTENT : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ARTICLE", "REVIEWED_CONTENT", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ARTICLE", "REVIEWED_CONTENT");
        }
    }
}
