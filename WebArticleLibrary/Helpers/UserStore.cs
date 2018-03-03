using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Drawing;
using System.IO;
using Microsoft.AspNet.Identity;
using WebArticleLibrary.Models;
using WebArticleLibrary.Model;

namespace WebArticleLibrary.Helpers
{
	public class UserStore : IUserStore<UserInfo>, IUserClaimStore<UserInfo>
	{
		private readonly ArticleLibraryContext db;
		public const Int32 expirationMinutes = 30;
		
		public UserStore(ArticleLibraryContext dbContext = null)
		{
			db = dbContext ?? new ArticleLibraryContext();
		}

		public UserInfo GetCurrentUserInfo()
		{
			ClaimsIdentity identity;
			Claim claim = null;

			if ((identity = HttpContext.Current.User.Identity as ClaimsIdentity) != null &&
				(claim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)) != null)
				return this.FindByIdAsync(claim.Value).Result;

			return null;
		}

		public Task AddClaimAsync(UserInfo user, Claim claim)
		{
			throw new NotImplementedException();
		}

		public Task CreateAsync(UserInfo user)
		{
			return Task.Run(() => InsertUser((USER)user));
		}

		public Task CreateAsync(UserInfo user, Guid confirmationId)
		{
			if (this.FindByNameAsync(user.name).Result != null)
				throw new Exception("There is already a user with the same name \"" + user.name + "\"");

			if (this.FindByEmailAsync(user.email).Result != null)
				throw new Exception("There is already a user with the same email");

			var u = (USER)user;
			u.CONFIRMATION_ID = confirmationId;
			u.EXPIRATION_DATE = DateTime.Now.AddMinutes(expirationMinutes);

			return Task.Run(() => InsertUser(u));
		}

		private void InsertUser(USER user)
		{
			user.INSERT_DATE = DateTime.Now;
			
			// The first registered user is always an administrator
			if (!db.USER.Any())
				user.STATUS = USER_STATUS.ADMINISTRATOR;

			db.USER.Add(user);
			db.SaveChanges();
		}

		public Task DeleteAsync(UserInfo user)
		{
			return Task.Run(() => {
                var _user = db.USER.FirstOrDefault(u => u.EMAIL == user.email);

                if (_user != null)
                {
                    db.USER.Remove(_user);
                    db.SaveChanges();
                }
            });
		}

		public void Dispose()
		{
			db.Dispose();
		}

		public Boolean ConfirmUser(Guid confirmationId)
		{
			var now = DateTime.Now;
			var user = db.USER.FirstOrDefault(u => u.CONFIRMATION_ID == confirmationId);

			if (user == null)
				return false;

			if (user.EXPIRATION_DATE < now)
			{
				db.USER.Remove(user);
				db.SaveChanges();
				return false;
			}

			user.EXPIRATION_DATE = null;
			user.CONFIRMATION_ID = null;
			db.SaveChanges();

			return true;
		}

		public Task<UserInfo> FindByIdAsync(string userId)
		{
			return Task<UserInfo>.Run(() => {
				var res = FindUser(userId);
				return res == null ? null : new UserInfo(res);
			});
		}

		private USER FindUser(String userId)
		{
			return db.USER.FirstOrDefault(u => u.ID.ToString() == userId && u.EXPIRATION_DATE == null);
		}

		public UserInfo MarkForResettingPassword(String email, out Guid confirmationId)
		{
			var user = db.USER.FirstOrDefault(u => u.EMAIL == email && u.EXPIRATION_DATE == null);

			if (user == null)
				throw new Exception("There is no user with such an email address");

			user.RESET_PASSWORD_EXPIRATION_DATE = DateTime.Now.AddMinutes(expirationMinutes);

			confirmationId = Guid.NewGuid();
			user.CONFIRMATION_ID = confirmationId;
			db.SaveChanges();

			return new UserInfo(user);
		}

		public Boolean ResetPassword(String newPassword, Guid confirmationId)
		{
			var now = DateTime.Now;
			var user = db.USER.FirstOrDefault(u => u.CONFIRMATION_ID == confirmationId);

			if (user == null)
				return false;

			var confirmed = user.RESET_PASSWORD_EXPIRATION_DATE > DateTime.Now;

			if (confirmed)
				user.HASH = UserInfo.GetHash(newPassword);

			user.RESET_PASSWORD_EXPIRATION_DATE = null;
			user.CONFIRMATION_ID = null;
			db.SaveChanges();

			return confirmed;
		}

		public IQueryable<USER> GetProperUsers()
		{
			var now = DateTime.Now;
			return db.USER.Where(u => u.EXPIRATION_DATE == null || u.EXPIRATION_DATE < now);
		}

		public Task<UserInfo> FindByNameAsync(string userName)
		{
			// During every login try it will remove all expired entries
			RemoveExpired();

			return Task<UserInfo>.Run(() => {
				var res = db.USER.FirstOrDefault(u => u.LOGIN == userName && u.EXPIRATION_DATE == null);
				return res == null ? null : new UserInfo(res);
			});
		}

		public Task<UserInfo> FindByEmailAsync(String email)
		{
			var now = DateTime.Now;

			// An email might be in the process of confirmation
			return Task<UserInfo>.Run(() => {
				var res = db.USER.FirstOrDefault(u => u.EMAIL == email && (
					u.EXPIRATION_DATE == null || now < u.EXPIRATION_DATE
				));
				return res == null ? null : new UserInfo(res);
			});
		}
		
		public Boolean ConfirmNewEmail(Guid confirmationId)
		{
			var now = DateTime.Now;
			var user = db.USER.FirstOrDefault(u => u.CONFIRMATION_ID == confirmationId);

			var confirmed = user.NEW_EMAIL != null && user.NEW_EMAIL_EXPIRATION_DATE > DateTime.Now;

			if (confirmed)
				user.EMAIL = user.NEW_EMAIL;

			user.NEW_EMAIL = null;
			user.NEW_EMAIL_EXPIRATION_DATE = null;
			user.CONFIRMATION_ID = null;
			db.SaveChanges();

			return confirmed;
		}

		public Task<IList<Claim>> GetClaimsAsync(UserInfo user)
		{
			return Task.Run(() =>
			{
				List<Claim> claims = new List<Claim>();
				claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserName));
				claims.Add(new Claim(ClaimTypes.Sid, user.Id.ToString()));

				return (IList<Claim>)claims;
			});
		}

		private void RemoveExpired()
		{
			var now = DateTime.Now;
			Boolean needSaving = false;
			var expired = db.USER.Where(u => u.EXPIRATION_DATE < now);

			if (expired.Any())
			{
				db.USER.RemoveRange(expired);
				needSaving = true;
			}

			expired = db.USER.Where(u => u.NEW_EMAIL_EXPIRATION_DATE < now);

			if (expired.Any())
			{
				db.USER.RemoveRange(expired);
				needSaving = true;
			}

			expired = db.USER.Where(u => u.RESET_PASSWORD_EXPIRATION_DATE < now);

			if (expired.Any())
			{
				db.USER.RemoveRange(expired);
				needSaving = true;
			}

			if (needSaving)
				db.SaveChanges();
		}

		public Guid? Update(UserInfo user)
		{
			var u = FindUser(user.Id);

			if (u == null)
				throw new Exception("No user was found");

			UserInfo _u;

			if ((_u = this.FindByNameAsync(user.name).Result) != null && _u.id != user.id)
				throw new Exception("There is already a user with the same name \"" + user.name + "\"");

			// Is it a bid to change password?
			if (user.newPassword != null)
			{
				var pHasher = new PasswordHasher();
				
				if (pHasher.VerifyHashedPassword(u.HASH, user.password) != PasswordVerificationResult.Success)
					throw new Exception("The old password is incorrect");
				else
					u.HASH = UserInfo.GetHash(user.newPassword);
			}

			// Is it a bid to change email address?
			if (user.email != u.EMAIL)
			{
				if (this.FindByEmailAsync(user.email).Result != null)
					throw new Exception("There is already a user with the same email");

				u.NEW_EMAIL = user.email;
				u.NEW_EMAIL_EXPIRATION_DATE = DateTime.Now.AddMinutes(expirationMinutes);
				u.CONFIRMATION_ID = Guid.NewGuid();
			}

			u.FIRST_NAME = user.firstName;
			u.LAST_NAME = user.lastName;
			u.PATRONYMIC_NAME = user.patronymicName;
			u.LOGIN = user.name;
			u.SHOW_PRIVATE_INFO = user.showPrivateInfo;
			u.STATUS = user.status;

			if (user.photo != null)
			{
				if (user.photo.Length == 0)
					u.PHOTO = null;
				else
				{
					var ms = new MemoryStream(user.photo);

					using (Image image = new Bitmap(ms))
					{
						Image.GetThumbnailImageAbort callback = new Image.GetThumbnailImageAbort(() => true);

						using (var photo = image.GetThumbnailImage(100, 100, callback, new IntPtr()))
						{
							var msPhoto = new MemoryStream();
							photo.Save(msPhoto, System.Drawing.Imaging.ImageFormat.Jpeg);
							msPhoto.Position = 0;

							Byte[] contents = new Byte[msPhoto.Length];
							msPhoto.Read(contents, 0, contents.Length);

							u.PHOTO = contents;
						}
					}
				}
			}

			db.SaveChanges();

			return u.CONFIRMATION_ID;
		}

		public Task RemoveClaimAsync(UserInfo user, Claim claim)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(UserInfo user)
		{
			throw new NotImplementedException();
		}
	}
}