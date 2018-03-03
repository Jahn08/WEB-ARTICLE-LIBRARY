using System;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using WebArticleLibrary.Models;
using WebArticleLibrary.Helpers;

namespace WebArticleLibrary.Controllers
{
	public class AuthenticationController: ApiController
	{
		[AllowAnonymous]
		[HttpPost]
		public IHttpActionResult LogIn(UserInfo user)
		{
			UserInfo u;
			HttpCookie cookie;
			var curContext = HttpContext.Current;

			using (var userStore = new UserStore())
			{
				if ((cookie = HttpContext.Current.Request.Cookies.Get(CustomAuthorizeAttribute.authUserCookie)) != null)
					return Ok<UserInfo>(userStore.GetCurrentUserInfo());

				u = userStore.FindByNameAsync(user.name).Result;
			}

			if (u != null)
			{
				if (u.status == Model.USER_STATUS.BANNED)
					return BadRequest("Your account was banned");

				var pHasher = new PasswordHasher();

				if (pHasher.VerifyHashedPassword(u.GetHash(), user.password) == PasswordVerificationResult.Success)
				{
					cookie = new HttpCookie(CustomAuthorizeAttribute.authUserCookie, CustomAuthorizeAttribute.EncryptId(u.id));

					curContext.Response.Cookies.Set(cookie);
					return Ok<UserInfo>(u);
				}
			}
			
			return Unauthorized();
		}

		[AllowAnonymous]
		[HttpGet]
		public IHttpActionResult LogOut()
		{
			HttpCookie cookie;

			if ((cookie = HttpContext.Current.Request.Cookies.Get(CustomAuthorizeAttribute.authUserCookie)) != null)
				HttpContext.Current.Response.Cookies.Remove(CustomAuthorizeAttribute.authUserCookie);

			return Ok();
		}

		[AllowAnonymous]
		[HttpPost]
		public IHttpActionResult Register(UserInfo user)
		{
			var confirmationId = Guid.NewGuid();

            using (var userStore = new UserStore())
            {
                userStore.CreateAsync(user, confirmationId).Wait();

                try
                {
                    MailHelper.SendConfirmationEmail("Registration", Properties.Resources.Authentication_Register_Message,
                        "confirmuser", user, confirmationId);
                }
                catch {
                    userStore.DeleteAsync(user).Wait();
                    throw;
                }
            }
            			
			return Ok();
		}

		[AllowAnonymous]
		[HttpGet]
		public IHttpActionResult Confirm(Guid confirmationId)
		{
			using (var userStore = new UserStore())
			{ 
				if (!userStore.ConfirmUser(confirmationId))
					return BadRequest("Either this account expired or it does not exist");
			
				return Ok();
			}
		}
		
		protected HttpContextWrapper GetHttpContextWrapper()
		{
			HttpContextWrapper httpContextWrapper = null;
			if (HttpContext.Current != null)
			{
				httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			}
			else if (Request.Properties.ContainsKey("MS_HttpContext"))
			{
				httpContextWrapper = (HttpContextWrapper)Request.Properties["MS_HttpContext"];
			}
			return httpContextWrapper;
		}
	}
}