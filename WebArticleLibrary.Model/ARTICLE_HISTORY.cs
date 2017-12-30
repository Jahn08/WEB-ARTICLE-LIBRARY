using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebArticleLibrary.Model
{
	[Table("ARTICLE_HISTORY")]
	[DataContract]
	public class ARTICLE_HISTORY
	{
		public ARTICLE_HISTORY()
		{
			USER_NOTIFICATION = new HashSet<USER_NOTIFICATION>();
		}

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[DataMember(Name = "id")]
		public Int32 ID { get; set; }
		
		[Required]
		[ForeignKey("ARTICLE")]
		[DataMember(Name = "articleId")]
		public Int32 ARTICLE_ID { get; set; }

		[Required]
		[ForeignKey("AUTHOR")]
		[DataMember(Name = "authorId")]
		public Int32 AUTHOR_ID { get; set; }

		[Required]
		[DataMember(Name = "object")]
		public String OBJECT { get; set; }

		[Required]
		[DataMember(Name = "insertDate")]
		public DateTime INSERT_DATE { get; set; }
		
		[DataMember(Name = "oldValue")]
		public String OLD_VALUE { get; set; }

		[DataMember(Name = "newValue")]
		public String NEW_VALUE { get; set; }

		[DataMember(Name = "objectId")]
		public Int32? OBJECT_ID { get; set; }
		
		[DataMember(Name = "objectType")]
		public HISTORY_OBJECT_TYPE OBJECT_TYPE { get; set; }

		public virtual USER AUTHOR { get; set; }

		public virtual ARTICLE ARTICLE { get; set; }

		public virtual ICollection<USER_NOTIFICATION> USER_NOTIFICATION { get; set; }
	}

	public enum HISTORY_OBJECT_TYPE
	{
		ARTICLE,
		AMENDMENT,
		COMMENT
	}
}
