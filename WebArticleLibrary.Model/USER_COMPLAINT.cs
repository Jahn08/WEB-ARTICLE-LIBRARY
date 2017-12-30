using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebArticleLibrary.Model
{
	[Table("USER_COMPLAINT")]
	[DataContract]
	public class USER_COMPLAINT
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		[DataMember(Name = "id")]
		public int ID { get; set; }

		[ForeignKey("AUTHOR")]
		[DataMember(Name = "authorId")]
		[Required]
		public int AUTHOR_ID { get; set; }

		[ForeignKey("ASSIGNED_TO")]
		[DataMember(Name = "assignedToId")]
		public int? ASSIGNED_TO_ID { get; set; }

		[ForeignKey("USER_COMMENT")]
		[DataMember(Name = "userCommentId")]
		public int? USER_COMMENT_ID { get; set; }

		[ForeignKey("ARTICLE")]
		[DataMember(Name = "articleId")]
		public int ARTICLE_ID { get; set; }

		[Required]
		[DataMember(Name = "text")]
		public String TEXT { get; set; }
		
		[Required]
		[DataMember(Name = "insertDate")]
		public DateTime INSERT_DATE { get; set; }
		
		[DataMember(Name = "status")]
		public COMPLAINT_STATUS STATUS { get; set; }

		public virtual USER AUTHOR { get; set; }

		public virtual USER_COMMENT USER_COMMENT { get; set; }

		public virtual ARTICLE ARTICLE { get; set; }

		public virtual USER ASSIGNED_TO { get; set; }
	}

	public enum COMPLAINT_STATUS {
		CREATED= 1,
		APPROVED = 4,
		REFUSED = 5
	}
}
