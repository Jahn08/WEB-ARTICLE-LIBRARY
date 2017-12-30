using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebArticleLibrary.Model
{
	[Table("USER_COMMENT")]
	[DataContract]
	public class USER_COMMENT
	{
		public USER_COMMENT()
		{
			RELATED_COMMENTS = new HashSet<USER_COMMENT>();
			USER_COMPLAINTS = new HashSet<USER_COMPLAINT>();
		}

		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		[DataMember(Name = "id")]
		public int ID { get; set; }

		[ForeignKey("AUTHOR")]
		[DataMember(Name = "authorId")]
		[Required]
		public int AUTHOR_ID { get; set; }

		[ForeignKey("RESPONSE_TO")]
		[DataMember(Name = "responseToId")]
		public int? RESPONSE_TO_ID { get; set; }

		[ForeignKey("ARTICLE")]
		[DataMember(Name = "articleId")]
		public int ARTICLE_ID { get; set; }

		[Required]
		[DataMember(Name = "content")]
		public Byte[] CONTENT { get; set; }
		
		[Required]
		[DataMember(Name = "insertDate")]
		public DateTime INSERT_DATE { get; set; }
		
		[DataMember(Name = "status")]
		public COMMENT_STATUS STATUS { get; set; }

		public virtual USER AUTHOR { get; set; }

		public virtual USER_COMMENT RESPONSE_TO { get; set; }

		public virtual ARTICLE ARTICLE { get; set; }	
		
		public virtual ICollection<USER_COMMENT> RELATED_COMMENTS { get; set; }

		public virtual ICollection<USER_COMPLAINT> USER_COMPLAINTS { get; set; }
	}

	public enum COMMENT_STATUS {
		CREATED = 1,
		BLOCKED = 6,
		DELETED
	}
}
