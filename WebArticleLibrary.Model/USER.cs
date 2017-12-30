namespace WebArticleLibrary.Model
{
	using System;
    using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Collections.Generic;

    [Table("USER")]
    public partial class USER
    {
		public USER()
		{
			ARTICLES = new HashSet<ARTICLE>();
			ASSIGNED_ARTICLES = new HashSet<ARTICLE>();
			AMENDMENTS = new HashSet<AMENDMENT>();
			ARTICLE_HISTORY = new HashSet<ARTICLE_HISTORY>();
			USER_COMMENTS = new HashSet<USER_COMMENT>();
			USER_ESTIMATES = new HashSet<USER_ESTIMATE>();
			USER_COMPLAINTS = new HashSet<USER_COMPLAINT>();
			ASSIGNED_USER_COMPLAINTS = new HashSet<USER_COMPLAINT>();
			USER_NOTIFICATION = new HashSet<USER_NOTIFICATION>();
		}

		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int ID { get; set; }

        [Required]
        [StringLength(10)]
        public string LOGIN { get; set; }

        [Required]
        [StringLength(50)]
        public string FIRST_NAME { get; set; }

        [Required]
        [StringLength(50)]
        public string LAST_NAME { get; set; }

        [StringLength(50)]
        public string PATRONYMIC_NAME { get; set; }

        [Required]
        [StringLength(50)]
        public string EMAIL { get; set; }

        [Required]
        [StringLength(250)]
        public string HASH { get; set; }

		[Required]
		public DateTime INSERT_DATE { get; set; }
		
		public DateTime? EXPIRATION_DATE { get; set; }

		public Guid? CONFIRMATION_ID { get; set; }

		public String NEW_EMAIL { get; set; }

		public DateTime? NEW_EMAIL_EXPIRATION_DATE { get; set; }

		public DateTime? RESET_PASSWORD_EXPIRATION_DATE { get; set; }

		public Byte[] PHOTO { get; set; }

		[Required]
		public USER_STATUS STATUS { get; set; }

		public Boolean SHOW_PRIVATE_INFO { get; set; }

		public virtual ICollection<ARTICLE> ARTICLES { get; set; }

		public virtual ICollection<ARTICLE> ASSIGNED_ARTICLES { get; set; }

		public virtual ICollection<AMENDMENT> AMENDMENTS { get; set; }

		public virtual ICollection<ARTICLE_HISTORY> ARTICLE_HISTORY { get; set; }

		public virtual ICollection<USER_COMMENT> USER_COMMENTS { get; set; }

		public virtual ICollection<USER_ESTIMATE> USER_ESTIMATES { get; set; }

		public virtual ICollection<USER_COMPLAINT> USER_COMPLAINTS { get; set; }
		
		public virtual ICollection<USER_COMPLAINT> ASSIGNED_USER_COMPLAINTS { get; set; }

		public virtual ICollection<USER_NOTIFICATION> USER_NOTIFICATION { get; set; }
		
		public String DisplayName {
			get {
				return String.Format("{0} {1}{2}", this.FIRST_NAME, this.LAST_NAME,
					this.PATRONYMIC_NAME == null ? null : " " + this.PATRONYMIC_NAME);
			}
		}
	}

	public enum USER_STATUS
	{
		USER,
		BANNED,
		ADMINISTRATOR = 5
	}
}
