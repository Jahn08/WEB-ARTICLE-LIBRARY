namespace WebArticleLibrary.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitializeContext : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.USER",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        LOGIN = c.String(nullable: false, maxLength: 10),
                        FIRST_NAME = c.String(nullable: false, maxLength: 50),
                        LAST_NAME = c.String(nullable: false, maxLength: 50),
                        PATRONYMIC_NAME = c.String(maxLength: 50),
                        EMAIL = c.String(nullable: false, maxLength: 50),
                        HASH = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.USER");
        }
    }
}
