using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace WebArticleLibrary.Model
{
	[Table("ARTICLE")]
	[DataContract]
	public class ARTICLE
	{
		public ARTICLE()
		{
			AMENDMENTS = new HashSet<AMENDMENT>();
			ARTICLE_HISTORY = new HashSet<ARTICLE_HISTORY>();
			USER_COMMENTS = new HashSet<USER_COMMENT>();
			USER_ESTIMATES = new HashSet<USER_ESTIMATE>();
			USER_COMPLAINTS = new HashSet<USER_COMPLAINT>();
		}

		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		[DataMember(Name="id")]
		public int ID { get; set; }

		[ForeignKey("AUTHOR")]
		[DataMember(Name = "authorId")]
		[Required]
		public int AUTHOR_ID { get; set; }

		[ForeignKey("ASSIGNED_TO")]
		[DataMember(Name = "assignedToId")]
		public int? ASSIGNED_TO_ID { get; set; }
	
		[Required]
		[StringLength(200)]
		[DataMember(Name = "name")]
		public String NAME { get; set; }
		
		[StringLength(500)]
		[DataMember(Name = "description")]
		public String DESCRIPTION { get; set; }

		[Required]
		[StringLength(50)]
		[DataMember(Name = "tags")]
		public String TAGS { get; set; }
		
		[Required]
		[DataMember(Name = "insertDate")]
		public DateTime INSERT_DATE { get; set; }
		
		[Required]
		[DataMember(Name = "status")]
		public ARTICLE_STATUS STATUS { get; set; }

		[Required]
		[DataMember(Name = "content")]
		public Byte[] CONTENT { get; set; }
		
		[DataMember(Name = "reviewedContent")]
		public Byte[] REVIEWED_CONTENT { get; set; }

		public virtual USER AUTHOR { get; set; }

		public virtual USER ASSIGNED_TO { get; set; }

		public virtual ICollection<AMENDMENT> AMENDMENTS { get; set; }

		public virtual ICollection<ARTICLE_HISTORY> ARTICLE_HISTORY { get; set; }

		public virtual ICollection<USER_COMMENT> USER_COMMENTS { get; set; }

		public virtual ICollection<USER_ESTIMATE> USER_ESTIMATES { get; set; }

		public virtual ICollection<USER_COMPLAINT> USER_COMPLAINTS { get; set; }

		public Int32 GetFinalEstimate()
		{
			return this.USER_ESTIMATES.Count(e => e.ESTIMATE == ESTIMATE_TYPE.POSITIVE) -
				this.USER_ESTIMATES.Count(e => e.ESTIMATE == ESTIMATE_TYPE.NEGATIVE);
		}
	}

	public enum ARTICLE_STATUS
	{
		DRAFT,
		CREATED,
		ON_REVIEW,
		ON_EDIT,
		APPROVED,
		ON_AMENDING = 8
	}
}
