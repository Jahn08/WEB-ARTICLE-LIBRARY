namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class USER_NOTIFICATION_TABLE : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.USER_NOTIFICATION",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TEXT = c.String(nullable: false, maxLength: 250),
                        INSERT_DATE = c.DateTime(nullable: false),
                        RECIPIENT_ID = c.Int(nullable: false),
                        ARTICLE_HISTORY_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.ARTICLE_HISTORY", t => t.ARTICLE_HISTORY_ID, cascadeDelete: true)
                .ForeignKey("dbo.USER", t => t.RECIPIENT_ID)
                .Index(t => t.RECIPIENT_ID)
                .Index(t => t.ARTICLE_HISTORY_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.USER_NOTIFICATION", "RECIPIENT_ID", "dbo.USER");
            DropForeignKey("dbo.USER_NOTIFICATION", "ARTICLE_HISTORY_ID", "dbo.ARTICLE_HISTORY");
            DropIndex("dbo.USER_NOTIFICATION", new[] { "ARTICLE_HISTORY_ID" });
            DropIndex("dbo.USER_NOTIFICATION", new[] { "RECIPIENT_ID" });
            DropTable("dbo.USER_NOTIFICATION");
        }
    }
}
