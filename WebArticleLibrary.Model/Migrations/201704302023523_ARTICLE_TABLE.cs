namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ARTICLE_TABLE : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ARTICLE",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AUTHOR_ID = c.Int(nullable: false),
                        NAME = c.String(nullable: false, maxLength: 200),
                        TAGS = c.String(nullable: false, maxLength: 50),
                        INSERT_DATE = c.DateTime(nullable: false),
                        STATUS = c.Int(nullable: false),
                        CONTENT = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.USER", t => t.AUTHOR_ID, cascadeDelete: true)
                .Index(t => t.AUTHOR_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ARTICLE", "AUTHOR_ID", "dbo.USER");
            DropIndex("dbo.ARTICLE", new[] { "AUTHOR_ID" });
            DropTable("dbo.ARTICLE");
        }
    }
}
