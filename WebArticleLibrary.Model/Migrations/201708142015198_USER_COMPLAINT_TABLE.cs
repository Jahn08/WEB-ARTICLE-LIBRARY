namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class USER_COMPLAINT_TABLE : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.USER_COMPLAINT",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AUTHOR_ID = c.Int(nullable: false),
                        ASSIGNED_TO_ID = c.Int(),
                        USER_COMMENT_ID = c.Int(),
                        ARTICLE_ID = c.Int(nullable: false),
                        TEXT = c.String(nullable: false),
                        INSERT_DATE = c.DateTime(nullable: false),
                        STATUS = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.ARTICLE", t => t.ARTICLE_ID, cascadeDelete: true)
                .ForeignKey("dbo.USER", t => t.ASSIGNED_TO_ID)
                .ForeignKey("dbo.USER", t => t.AUTHOR_ID)
                .ForeignKey("dbo.USER_COMMENT", t => t.USER_COMMENT_ID)
                .Index(t => t.AUTHOR_ID)
                .Index(t => t.ASSIGNED_TO_ID)
                .Index(t => t.USER_COMMENT_ID)
                .Index(t => t.ARTICLE_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.USER_COMPLAINT", "USER_COMMENT_ID", "dbo.USER_COMMENT");
            DropForeignKey("dbo.USER_COMPLAINT", "AUTHOR_ID", "dbo.USER");
            DropForeignKey("dbo.USER_COMPLAINT", "ASSIGNED_TO_ID", "dbo.USER");
            DropForeignKey("dbo.USER_COMPLAINT", "ARTICLE_ID", "dbo.ARTICLE");
            DropIndex("dbo.USER_COMPLAINT", new[] { "ARTICLE_ID" });
            DropIndex("dbo.USER_COMPLAINT", new[] { "USER_COMMENT_ID" });
            DropIndex("dbo.USER_COMPLAINT", new[] { "ASSIGNED_TO_ID" });
            DropIndex("dbo.USER_COMPLAINT", new[] { "AUTHOR_ID" });
            DropTable("dbo.USER_COMPLAINT");
        }
    }
}
