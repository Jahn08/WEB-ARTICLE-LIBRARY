namespace WebArticleLibrary.Model.Migrations
{
    using System.Data.Entity;
    using System.Data.Entity.Migrations;

    public sealed class Configuration : DbMigrationsConfiguration<ArticleLibraryContext>
    {
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}

		static public void UpdateDatabase()
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<ArticleLibraryContext, Configuration>());
		}

		protected override void Seed(ArticleLibraryContext context)
		{
			base.Seed(context);
		}
	}
}
