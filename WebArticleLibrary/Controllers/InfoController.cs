using System.Web.Http;
using System.Configuration;

namespace WebArticleLibrary.Controllers
{
	[AllowAnonymous]
	public class InfoController: ApiController
	{
		[HttpGet]
		public IHttpActionResult GetBasicInfo()
		{
			return Ok(new {
				fax = ConfigurationManager.AppSettings["fax"],
				phone = ConfigurationManager.AppSettings["phone"],
				mail = ConfigurationManager.AppSettings["mail"],
				youtubeLink = ConfigurationManager.AppSettings["youtubeLink"],
				facebookLink = ConfigurationManager.AppSettings["facebookLink"]
			});
		}

		[HttpGet]
		public IHttpActionResult GetAboutUsInfo()
		{
			return Ok(new {
				aboutUs = ConfigurationManager.AppSettings["aboutUs"]
			});
		}
	}
}