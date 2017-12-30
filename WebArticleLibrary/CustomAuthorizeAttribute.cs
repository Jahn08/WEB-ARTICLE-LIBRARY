using System;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Security.Claims;
using System.Threading;
using WebArticleLibrary.Model;
using WebArticleLibrary.Helpers;

namespace WebArticleLibrary
{
	public class CustomAuthorizeAttribute : AuthorizeAttribute
	{
		public const String authUserCookie = "Auth_Id";
		public const String adminRole = "ADMINISTRATOR";

		private static Byte[] _key;
		private static Byte[] _IV;
		
		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			HttpCookie cookie;
			var curContext = HttpContext.Current;

			if (curContext != null && (cookie = curContext.Request.Cookies.Get(authUserCookie)) != null)
			{
				UserStore us = new UserStore();
				Int32 id;

				try
				{
					id = DecryptId(cookie.Value);
				}
				catch (CryptographicException)
				{
					curContext.Response.Cookies.Remove(authUserCookie);
					return false;
				}

				if (id == -1)
					return false;

				var user = us.FindByIdAsync(id.ToString()).Result;

				if (user != null)
				{
					USER_STATUS requirement;

					if (user.status == USER_STATUS.BANNED)
						return false;
					else if (!String.IsNullOrEmpty(Roles) && (!Enum.TryParse<USER_STATUS>(Roles, out requirement) ||
						user.status != requirement))
						return false;

					var identity = new ClaimsIdentity(us.GetClaimsAsync(user).Result);
					var principal = new ClaimsPrincipal(identity);

					Thread.CurrentPrincipal = principal;

					if (HttpContext.Current != null)
						HttpContext.Current.User = principal;

					return true;
				}
			}

			return false;
		}
		
		static CustomAuthorizeAttribute()
		{
			using (var c = new RC2CryptoServiceProvider())
			{
				_key = c.Key;
				_IV = c.IV;
			}
		}

		public static String EncryptId(Int32 id)
		{
			String value;
			String info = String.Format("{0}_{1}", Guid.NewGuid().ToString(), id.ToString());

			using (var c = new RC2CryptoServiceProvider())
			{
				var encr = c.CreateEncryptor(_key, _IV);

				using (MemoryStream mem = new MemoryStream())
				{
					CryptoStream crStream = new CryptoStream(mem, encr, CryptoStreamMode.Write);

					var bytes = System.Text.Encoding.UTF8.GetBytes(info);
					crStream.Write(bytes, 0, bytes.Length);
					crStream.FlushFinalBlock();

					value = Convert.ToBase64String(mem.ToArray());
				}
			}
			
			return value;
		}

		private static Int32 DecryptId(String info)
		{
			String value = null;
			Int32 id = -1;
			var bytes = Convert.FromBase64String(info);

			using (var c = new RC2CryptoServiceProvider())
			{
				var decr = c.CreateDecryptor(_key, _IV);

				using (MemoryStream mem = new MemoryStream(bytes))
				{
					CryptoStream crStream = new CryptoStream(mem, decr, CryptoStreamMode.Read);

					var outcome = new Byte[bytes.Length];
					crStream.Read(outcome, 0, outcome.Length);

					value = System.Text.Encoding.UTF8.GetString(outcome);
				}
			}

			if (!String.IsNullOrEmpty(value))
			{
				var number = value.Split('_').Last();
				Int32.TryParse(number, out id);
			}

			return id;
		}
	}
}