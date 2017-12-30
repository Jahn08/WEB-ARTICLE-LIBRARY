using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace WebArticleLibrary.Model
{
	[Table("AMENDMENT")]
	[DataContract]
	public class AMENDMENT
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		[DataMember(Name="id")]
		public Int32 ID { get; set; }

		[ForeignKey("AUTHOR")]
		[DataMember(Name="authorId")]
		[Required]
		public Int32 AUTHOR_ID { get; set; }

		[ForeignKey("ARTICLE")]
		[DataMember(Name = "articleId")]
		[Required]
		public Int32 ARTICLE_ID { get; set; }

		[Required]
		[DataMember(Name = "content")]
		public String CONTENT { get; set; }
		
		[DataMember(Name = "resolved")]
		public Boolean RESOLVED { get; set; }
		
		[DataMember(Name = "archived")]
		public Boolean ARCHIVED { get; set; }

		[Required]
		[DataMember(Name = "insertDate")]
		public DateTime INSERT_DATE { get; set; }

		public virtual ARTICLE ARTICLE { get; set; }

		public virtual USER AUTHOR { get; set; }
	}
}
