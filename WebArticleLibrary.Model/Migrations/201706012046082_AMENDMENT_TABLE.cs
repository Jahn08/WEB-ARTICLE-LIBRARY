namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AMENDMENT_TABLE : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AMENDMENT",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AUTHOR_ID = c.Int(nullable: false),
                        ARTICLE_ID = c.Int(nullable: false),
                        CONTENT = c.String(nullable: false),
                        RESOLVED = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.ARTICLE", t => t.ARTICLE_ID, cascadeDelete: true)
                .ForeignKey("dbo.USER", t => t.AUTHOR_ID)
                .Index(t => t.AUTHOR_ID)
                .Index(t => t.ARTICLE_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AMENDMENT", "AUTHOR_ID", "dbo.USER");
            DropForeignKey("dbo.AMENDMENT", "ARTICLE_ID", "dbo.ARTICLE");
            DropIndex("dbo.AMENDMENT", new[] { "ARTICLE_ID" });
            DropIndex("dbo.AMENDMENT", new[] { "AUTHOR_ID" });
            DropTable("dbo.AMENDMENT");
        }
    }
}
