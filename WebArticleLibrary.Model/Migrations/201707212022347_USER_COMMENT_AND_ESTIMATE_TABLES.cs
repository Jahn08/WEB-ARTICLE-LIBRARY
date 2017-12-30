namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class USER_COMMENT_AND_ESTIMATE_TABLES : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.USER_COMMENT",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AUTHOR_ID = c.Int(nullable: false),
                        RESPONSE_TO_ID = c.Int(),
                        ARTICLE_ID = c.Int(nullable: false),
                        CONTENT = c.Binary(nullable: false),
                        INSERT_DATE = c.DateTime(nullable: false),
                        STATUS = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.ARTICLE", t => t.ARTICLE_ID, cascadeDelete: true)
                .ForeignKey("dbo.USER", t => t.AUTHOR_ID)
                .ForeignKey("dbo.USER_COMMENT", t => t.RESPONSE_TO_ID)
                .Index(t => t.AUTHOR_ID)
                .Index(t => t.RESPONSE_TO_ID)
                .Index(t => t.ARTICLE_ID);
            
            CreateTable(
                "dbo.USER_ESTIMATE",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AUTHOR_ID = c.Int(nullable: false),
                        ARTICLE_ID = c.Int(nullable: false),
                        ESTIMATE = c.Int(nullable: false),
                        INSERT_DATE = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.ARTICLE", t => t.ARTICLE_ID, cascadeDelete: true)
                .ForeignKey("dbo.USER", t => t.AUTHOR_ID)
                .Index(t => t.AUTHOR_ID)
                .Index(t => t.ARTICLE_ID);
            
            AddColumn("dbo.ARTICLE_HISTORY", "OBJECT_TYPE", c => c.Int(nullable: false));
            AddColumn("dbo.USER", "PHOTO", c => c.Binary());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.USER_ESTIMATE", "AUTHOR_ID", "dbo.USER");
            DropForeignKey("dbo.USER_ESTIMATE", "ARTICLE_ID", "dbo.ARTICLE");
            DropForeignKey("dbo.USER_COMMENT", "RESPONSE_TO_ID", "dbo.USER_COMMENT");
            DropForeignKey("dbo.USER_COMMENT", "AUTHOR_ID", "dbo.USER");
            DropForeignKey("dbo.USER_COMMENT", "ARTICLE_ID", "dbo.ARTICLE");
            DropIndex("dbo.USER_ESTIMATE", new[] { "ARTICLE_ID" });
            DropIndex("dbo.USER_ESTIMATE", new[] { "AUTHOR_ID" });
            DropIndex("dbo.USER_COMMENT", new[] { "ARTICLE_ID" });
            DropIndex("dbo.USER_COMMENT", new[] { "RESPONSE_TO_ID" });
            DropIndex("dbo.USER_COMMENT", new[] { "AUTHOR_ID" });
            DropColumn("dbo.USER", "PHOTO");
            DropColumn("dbo.ARTICLE_HISTORY", "OBJECT_TYPE");
            DropTable("dbo.USER_ESTIMATE");
            DropTable("dbo.USER_COMMENT");
        }
    }
}
