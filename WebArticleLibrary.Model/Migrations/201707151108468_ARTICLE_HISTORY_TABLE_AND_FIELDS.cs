namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ARTICLE_HISTORY_TABLE_AND_FIELDS : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ARTICLE_HISTORY",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ARTICLE_ID = c.Int(nullable: false),
                        AUTHOR_ID = c.Int(nullable: false),
                        OBJECT = c.String(nullable: false),
                        INSERT_DATE = c.DateTime(nullable: false),
                        OLD_VALUE = c.String(),
                        NEW_VALUE = c.String(),
                        OBJECT_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.ARTICLE", t => t.ARTICLE_ID, cascadeDelete: true)
                .ForeignKey("dbo.USER", t => t.AUTHOR_ID)
                .Index(t => t.ARTICLE_ID)
                .Index(t => t.AUTHOR_ID);
            
            AddColumn("dbo.AMENDMENT", "INSERT_DATE", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ARTICLE_HISTORY", "AUTHOR_ID", "dbo.USER");
            DropForeignKey("dbo.ARTICLE_HISTORY", "ARTICLE_ID", "dbo.ARTICLE");
            DropIndex("dbo.ARTICLE_HISTORY", new[] { "AUTHOR_ID" });
            DropIndex("dbo.ARTICLE_HISTORY", new[] { "ARTICLE_ID" });
            DropColumn("dbo.AMENDMENT", "INSERT_DATE");
            DropTable("dbo.ARTICLE_HISTORY");
        }
    }
}
