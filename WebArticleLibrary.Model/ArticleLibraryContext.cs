namespace WebArticleLibrary.Model
{
	using System;
	using System.Data.Entity;
	using System.Linq;
	using WebArticleLibrary.Model.Migrations;

	public partial class ArticleLibraryContext : DbContext, IDisposable
	{
		public ArticleLibraryContext()
			: base("name=WebArticleLib_DBConnection")
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<ArticleLibraryContext, Configuration>());
		}

		public delegate void OnNotificationAdded(USER_NOTIFICATION[] etities);
		public event OnNotificationAdded OnNotificationAddedEvent;

		public override int SaveChanges()
		{
			var outcome = base.SaveChanges();

			var _event = OnNotificationAddedEvent;

			if (_event != null)
			{
				var etities = this.ChangeTracker.Entries<USER_NOTIFICATION>().Select(e => e.Entity).ToArray();

				if (etities.Any())
					_event(etities);
			}

			return outcome;
		}

		public void Close()
		{
			this.Dispose();
		}

		public virtual DbSet<USER> USER { get; set; }
		public virtual DbSet<ARTICLE> ARTICLE { get; set; }
		public virtual DbSet<AMENDMENT> AMENDMENTS { get; set; }
		public virtual DbSet<ARTICLE_HISTORY> ARTICLE_HISTORY { get; set; }
		public virtual DbSet<USER_COMMENT> USER_COMMENT { get; set; }
		public virtual DbSet<USER_ESTIMATE> USER_ESTIMATE { get; set; }
		public virtual DbSet<USER_COMPLAINT> USER_COMPLAINT { get; set; }
		public virtual DbSet<USER_NOTIFICATION> USER_NOTIFICATION { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<USER_COMPLAINT>().HasRequired(a => a.AUTHOR).WithMany(u => u.USER_COMPLAINTS)
				.HasForeignKey(a => a.AUTHOR_ID).WillCascadeOnDelete(false);

			modelBuilder.Entity<USER_COMPLAINT>().HasRequired(a => a.ARTICLE).WithMany(u => u.USER_COMPLAINTS)
				.HasForeignKey(a => a.ARTICLE_ID).WillCascadeOnDelete();

			modelBuilder.Entity<USER_COMPLAINT>().HasOptional(a => a.USER_COMMENT).WithMany(u => u.USER_COMPLAINTS)
				.HasForeignKey(a => a.USER_COMMENT_ID).WillCascadeOnDelete(false);

			modelBuilder.Entity<USER_COMPLAINT>().HasOptional(a => a.ASSIGNED_TO).WithMany(u => u.ASSIGNED_USER_COMPLAINTS)
				.HasForeignKey(a => a.ASSIGNED_TO_ID).WillCascadeOnDelete(false);

			modelBuilder.Entity<USER_COMMENT>().HasRequired(a => a.AUTHOR).WithMany(u => u.USER_COMMENTS)
				.HasForeignKey(a => a.AUTHOR_ID).WillCascadeOnDelete(false);

			modelBuilder.Entity<USER_COMMENT>().HasRequired(a => a.ARTICLE).WithMany(u => u.USER_COMMENTS)
				.HasForeignKey(a => a.ARTICLE_ID).WillCascadeOnDelete();

			modelBuilder.Entity<USER_COMMENT>().HasOptional(a => a.RESPONSE_TO).WithMany(u => u.RELATED_COMMENTS)
				.HasForeignKey(a => a.RESPONSE_TO_ID).WillCascadeOnDelete(false);

			modelBuilder.Entity<USER_ESTIMATE>().HasRequired(a => a.AUTHOR).WithMany(u => u.USER_ESTIMATES)
				.HasForeignKey(a => a.AUTHOR_ID).WillCascadeOnDelete(false);

			modelBuilder.Entity<USER_ESTIMATE>().HasRequired(a => a.ARTICLE).WithMany(u => u.USER_ESTIMATES)
				.HasForeignKey(a => a.ARTICLE_ID).WillCascadeOnDelete();

			modelBuilder.Entity<ARTICLE>().HasRequired(a => a.AUTHOR).WithMany(u => u.ARTICLES)
				.HasForeignKey(a => a.AUTHOR_ID).WillCascadeOnDelete();

			modelBuilder.Entity<ARTICLE>().HasOptional(a => a.ASSIGNED_TO).WithMany(u => u.ASSIGNED_ARTICLES)
				.HasForeignKey(a => a.ASSIGNED_TO_ID);
			
			modelBuilder.Entity<AMENDMENT>().HasRequired(a => a.AUTHOR).WithMany(u => u.AMENDMENTS)
				.HasForeignKey(a => a.AUTHOR_ID).WillCascadeOnDelete(false);

			modelBuilder.Entity<AMENDMENT>().HasRequired(a => a.ARTICLE).WithMany(u => u.AMENDMENTS)
				.HasForeignKey(a => a.ARTICLE_ID);

			modelBuilder.Entity<ARTICLE_HISTORY>().HasRequired(a => a.AUTHOR).WithMany(u => u.ARTICLE_HISTORY)
				.HasForeignKey(a => a.AUTHOR_ID).WillCascadeOnDelete(false);

			modelBuilder.Entity<ARTICLE_HISTORY>().HasRequired(a => a.ARTICLE).WithMany(u => u.ARTICLE_HISTORY)
				.HasForeignKey(a => a.ARTICLE_ID);
			
			modelBuilder.Entity<USER_NOTIFICATION>().HasRequired(a => a.ARTICLE_HISTORY).WithMany(h => h.USER_NOTIFICATION)
				.HasForeignKey(a => a.ARTICLE_HISTORY_ID);

			modelBuilder.Entity<USER_NOTIFICATION>().HasRequired(a => a.RECIPIENT).WithMany(u => u.USER_NOTIFICATION)
				.HasForeignKey(a => a.RECIPIENT_ID).WillCascadeOnDelete(false);
		}
	}
}
