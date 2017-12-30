using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebArticleLibrary.Model
{
	[Table("USER_NOTIFICATION")]
	public class USER_NOTIFICATION
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int ID { get; set; }

		[Required]
		[MaxLength(250)]
		public String TEXT { get; set; }
		
		[Required]
		public DateTime INSERT_DATE { get; set; }

		[ForeignKey("RECIPIENT")]
		[Required]
		public int RECIPIENT_ID { get; set; }

		[ForeignKey("ARTICLE_HISTORY")]
		[Required]
		public int ARTICLE_HISTORY_ID { get; set; }

		public virtual USER RECIPIENT { get; set; }

		public virtual ARTICLE_HISTORY ARTICLE_HISTORY { get; set; }
	}
}