using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebArticleLibrary.Model
{
	[Table("USER_ESTIMATE")]
	[DataContract]
	public class USER_ESTIMATE
	{	
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		[DataMember(Name = "id")]
		public int ID { get; set; }

		[ForeignKey("AUTHOR")]
		[DataMember(Name = "authorId")]
		[Required]
		public int AUTHOR_ID { get; set; }
		
		[ForeignKey("ARTICLE")]
		[DataMember(Name = "articleId")]
		public int ARTICLE_ID { get; set; }
		
		[Required]
		[DataMember(Name = "estimate")]
		public ESTIMATE_TYPE ESTIMATE { get; set; }

		[Required]
		[DataMember(Name = "insertDate")]
		public DateTime INSERT_DATE { get; set; }
		
		public virtual USER AUTHOR { get; set; }

		public virtual ARTICLE ARTICLE { get; set; }		
	}

	public enum ESTIMATE_TYPE {
		NONE,
		POSITIVE,
		NEGATIVE
	}
}
