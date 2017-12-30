using System;
using System.Runtime.Serialization;
using Microsoft.AspNet.Identity;
using WebArticleLibrary.Model;

namespace WebArticleLibrary.Models
{
	[DataContract]
	public class UserInfo: IUser<String>
	{
		public UserInfo() { }
		
		public UserInfo(USER user, Boolean considerPrivateData = false)
		{
			this.id = user.ID;
			this.UserName = user.LOGIN;
			this.status = user.STATUS;
			this.insertDate = user.INSERT_DATE;
			this.showPrivateInfo = user.SHOW_PRIVATE_INFO;
			this.photo = user.PHOTO;

			if (!considerPrivateData || this.showPrivateInfo)
			{
				this.hash = user.HASH;
				this.email = user.EMAIL;
				this.expirationDate = user.EXPIRATION_DATE;
				this.newEmailExpirationDate = user.NEW_EMAIL_EXPIRATION_DATE;
				this.resetPasswordExpirationDate = user.RESET_PASSWORD_EXPIRATION_DATE;
				this.firstName = user.FIRST_NAME;
				this.lastName = user.LAST_NAME;
				this.patronymicName = user.PATRONYMIC_NAME;
			}
		}
		
		public static explicit operator USER(UserInfo user)
		{
			Int32 _id;
			Int32.TryParse(user.Id, out _id);

			return new USER {
				ID = _id,
				LOGIN = user.UserName,
				FIRST_NAME = user.firstName,
				LAST_NAME = user.lastName,
				PATRONYMIC_NAME = user.patronymicName,
				EMAIL = user.email,
				HASH = user.password == null ? null: user.GetHash(),
				STATUS = user.status,
				INSERT_DATE = user.insertDate,
				EXPIRATION_DATE = user.expirationDate,
				NEW_EMAIL_EXPIRATION_DATE = user.newEmailExpirationDate,
				RESET_PASSWORD_EXPIRATION_DATE = user.resetPasswordExpirationDate,
				PHOTO = user.photo
			};
		}
		
		[DataMember]
		public USER_STATUS status { get; set; }

		[DataMember]
		public Int32 id { get; set; }

		[DataMember]
		public String name { get; set; }

		[DataMember]
		public String password { get; set; }

		[DataMember]
		public String newPassword { get; set; }

		[DataMember]
		public String firstName { get; set; }

		[DataMember]
		public String lastName { get; set; }

		[DataMember]
		public String patronymicName { get; set; }

		[DataMember]
		public String email { get; set; }

		[DataMember]
		public Byte[] photo { get; set; }

		[DataMember]
		public Boolean showPrivateInfo { get; set; }

		[DataMember]
		public DateTime insertDate { get; set; }

		[DataMember]
		public DateTime? expirationDate { get; set; }

		[DataMember]
		public DateTime? newEmailExpirationDate { get; set; }

		[DataMember]
		public DateTime? resetPasswordExpirationDate { get; set; }

		private String hash;
		
		public String GetHash() {
			if (!String.IsNullOrEmpty(this.hash))
				return this.hash;

			return this.hash = GetHash(this.password);
		}

		public static String GetHash(String val)
		{
			if (String.IsNullOrEmpty(val))
				throw new NullReferenceException("There is no field to be hashed");

			var pHasher = new PasswordHasher();
			return pHasher.HashPassword(val);
		}

		public String Id
		{
			get
			{
				return id.ToString();
			}
		}

		public string UserName
		{
			get
			{
				return name;
			}

			set
			{
				name = value;
			}
		}
	}
}