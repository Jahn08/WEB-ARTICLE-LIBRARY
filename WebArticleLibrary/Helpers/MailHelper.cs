using System;
using System.Web;
using System.Net.Mail;
using System.Configuration;
using System.Net;
using System.Reflection;
using WebArticleLibrary.Models;

namespace WebArticleLibrary.Helpers
{
	public static class MailHelper
	{
		public static void SendConfirmationEmail(String subject, String msgTemplate, String redirectPageName, UserInfo userInfo, Guid confirmationId)
		{
			SendMessage(userInfo.email, subject, String.Format(msgTemplate, userInfo.firstName, userInfo.lastName,
				HttpContext.Current.Request.UrlReferrer.OriginalString + 
				"#/" + redirectPageName + "/" + confirmationId.ToString(),
				DateTime.Now.AddMinutes(UserStore.expirationMinutes).ToString("dd.MM.yyyy HH:mm")));
		}
		
		public static void SendNotificationEmail(String subject, String msgTemplate, String redirectPageName, UserInfo userInfo, String reason)
		{
			SendMessage(userInfo.email, subject, String.Format(msgTemplate, userInfo.firstName, userInfo.lastName,
				reason, HttpContext.Current.Request.UrlReferrer.OriginalString + "#/" + redirectPageName));
		}

		private static SmtpClient SendMessage(String email, String subject, String body)
		{
			SmtpClient smtp = new SmtpClient(ConfigurationManager.AppSettings["smtpHost"],
				Int32.Parse(ConfigurationManager.AppSettings["smtpPort"]));
			smtp.EnableSsl = true;
			smtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["smtpUserName"],
				ConfigurationManager.AppSettings["smtpPassword"]);

			String productName = ((AssemblyProductAttribute)Assembly.GetExecutingAssembly()
				.GetCustomAttribute(typeof(AssemblyProductAttribute))).Product;

			MailMessage msg = new MailMessage(ConfigurationManager.AppSettings["smtpUserName"],
				email, productName + " " + subject, body);
			msg.IsBodyHtml = true;
			smtp.Send(msg);

			return smtp;
		}
	}
}