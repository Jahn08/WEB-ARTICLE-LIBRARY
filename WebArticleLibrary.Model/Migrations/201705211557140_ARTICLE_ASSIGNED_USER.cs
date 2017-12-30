namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ARTICLE_ASSIGNED_USER : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ARTICLE", "ASSIGNED_TO_ID", c => c.Int());
            CreateIndex("dbo.ARTICLE", "ASSIGNED_TO_ID");
            AddForeignKey("dbo.ARTICLE", "ASSIGNED_TO_ID", "dbo.USER", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ARTICLE", "ASSIGNED_TO_ID", "dbo.USER");
            DropIndex("dbo.ARTICLE", new[] { "ASSIGNED_TO_ID" });
            DropColumn("dbo.ARTICLE", "ASSIGNED_TO_ID");
        }
    }
}
