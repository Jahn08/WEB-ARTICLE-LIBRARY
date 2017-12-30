using System;
using System.Linq;
using System.Web.Http;
using System.Collections.Generic;
using WebArticleLibrary.Model;
using WebArticleLibrary.Models;
using WebArticleLibrary.Helpers;

namespace WebArticleLibrary.Controllers
{
    public class UserInfoController : ApiController
    {
		const Int32 pageLength = 10;

		[AllowAnonymous]
		[HttpGet]
		public IHttpActionResult ResetPassword(String email)
		{
			Guid confirmationId;
			UserInfo user;

			using (var store = new UserStore())
			{
				user = store.MarkForResettingPassword(email, out confirmationId);
			}
			
			MailHelper.SendConfirmationEmail("Reset Password", Properties.Resources.UserInfo_ResetPassword_Message,
				"resetpassword", user, confirmationId);

			return Ok();
		}
		
		[AllowAnonymous]
		[HttpPost]
		public IHttpActionResult ReplacePassword(ReplacePasswordForm formData)
		{
			using (var store = new UserStore())
			{
				if (!store.ResetPassword(formData.newPassword, formData.confirmationId))
					return BadRequest("Either this link expired or the password does not need to be resetted");
			}
						
			return Ok();
		}

		[HttpGet]
		public IHttpActionResult GetUsers(Int32 page = 1, String name = null, String login = null,
			String email = null, USER_STATUS? status = null, 
			ColumnIndex colIndex = ColumnIndex.NAME, Boolean asc = true)
		{
			UserInfo user;
			IEnumerable<UserInfo> userData;
			Dictionary<Int32, Int32> cmntNumber = null;
			Dictionary<Int32, Int32> artNumber = null;
			Int32 dataCount = 0;

			using (var db = new Model.ArticleLibraryContext())
			{
				UserStore store = new UserStore(db);
			
				if ((user = GetInfoInternally(store)) == null)
					return Unauthorized();

				var curUserId = user.id;
				var users = store.GetProperUsers().Where(u => u.ID != curUserId);

				if (status != null)
				{
					users = users.Where(u => u.STATUS == status);
				}

				if (login != null)
				{
					var filterLogin = login.ToUpper();
					users = users.Where(u => u.LOGIN.ToUpper().Contains(filterLogin));
				}

				if (email != null || name != null)
				{
					users = users.Where(u => u.SHOW_PRIVATE_INFO);

					if (email != null)
					{
						var filterEmail = email.ToUpper();
						users = users.Where(u => u.EMAIL.ToUpper().Contains(filterEmail));
					}

					if (name != null)
					{
						var filterName = name.ToUpper();
						users = users.Where(u => (u.FIRST_NAME + u.LAST_NAME + u.PATRONYMIC_NAME).ToUpper().Contains(filterName));
					}
				}

				users = OrderUsers(users, colIndex, asc);
				dataCount = users.Count();
				users = users.Skip((page - 1) * pageLength).Take(pageLength);

				cmntNumber = (from uc in db.USER_COMMENT.Where(c => c.STATUS != COMMENT_STATUS.DELETED)
							 join uId in users.Select(us => us.ID) on uc.AUTHOR_ID equals uId
								group uc by uc.AUTHOR_ID into g
								select g).ToDictionary(k => k.Key, v => v.Count());
				artNumber = (from art in db.ARTICLE.Where(a => a.STATUS != ARTICLE_STATUS.DRAFT)
							  join uId in users.Select(us => us.ID) on art.AUTHOR_ID equals uId
							  group art by art.AUTHOR_ID into g
							  select g).ToDictionary(k => k.Key, v => v.Count());

				userData = users.ToArray().Select(u => new UserInfo(u, true));
			}

			return Ok(new
			{
				data = userData,
				dataCount = dataCount,
				cmntNumber = cmntNumber,
				artNumber = artNumber,
				pageLength = pageLength
			});
		}

		private IQueryable<USER> OrderUsers(IQueryable<USER> source, ColumnIndex colIndex, Boolean asc)
		{
			switch (colIndex)
			{
				case ColumnIndex.NAME:
					source = asc ? source.OrderBy(s => s.LOGIN) : source.OrderByDescending(s => s.LOGIN);
					break;
				case ColumnIndex.STATUS:
					source = asc ? source.OrderBy(s => s.STATUS) : source.OrderByDescending(s => s.STATUS);
					break;
			}

			return source;
		}

		[HttpGet]
		[CustomAuthorize(Roles = CustomAuthorizeAttribute.adminRole)]
		public IHttpActionResult SetUserStatus(String userId, Model.USER_STATUS status)
		{
			UserInfo userForUpd;

			using (UserStore store = new UserStore())
			{
				userForUpd = store.FindByIdAsync(userId).Result;

				if (userForUpd == null)
					return BadRequest("There is no user with such an id");

				if (userForUpd.status == status)
					return BadRequest("The user already has this status");

				userForUpd.status = status;
				store.Update(userForUpd);
			}

			MailHelper.SendNotificationEmail("Status Updated", Properties.Resources.UserInfo_AccountChanges_Message,
				"aboutus", userForUpd,
				"your status has been changed to \"" + Enum.GetName(typeof(Model.USER_STATUS), status) + "\"");

			return Ok();
		}

		[HttpGet]
		public IHttpActionResult GetInfo(String userId, Boolean getPhoto = false)
		{
			UserInfo user;

			using (var store = new UserStore())
			{
				if ((user = GetInfoInternally(store, userId)) == null)
					return BadRequest("No user was found");
			}

			if (!getPhoto)
				user.photo = null;

			return Ok<UserInfo>(user);
		}

		private UserInfo GetInfoInternally(UserStore store, String userId = null)
		{			
			// The usage of the current user
			if (String.IsNullOrEmpty(userId))
				return store.GetCurrentUserInfo();

			return store.FindByIdAsync(userId).Result;
		}

		[HttpPost]
		public IHttpActionResult SaveInfo(UserInfo userInfo)
		{
			Guid? confirmationId;
			
			using (var store = new UserStore())
			{
				confirmationId = store.Update(userInfo);
			}

			// Is it a bid to change email address?
			if (confirmationId != null)
				MailHelper.SendConfirmationEmail("Change Email Address", Properties.Resources.UserInfo_NewEmail_Message,
					"confirmemail", userInfo, confirmationId.Value);

			return Ok();
		}

		[AllowAnonymous]
		[HttpGet]
		public IHttpActionResult ConfirmEmail(Guid confirmationId)
		{
			using (var userStore = new UserStore())
			{ 		
				if (!userStore.ConfirmNewEmail(confirmationId))
					return BadRequest("Either this link expired or the email does not need to be confirmed");
			}

			return Ok();
		}

	}
}
