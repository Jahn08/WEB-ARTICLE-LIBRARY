using System.Web.Http.Filters;
using WebArticleLibrary.Helpers;

namespace WebArticleLibrary.App_Start
{
	public class HandleErrorAttribute: ExceptionFilterAttribute
	{
		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			Logger.Error(actionExecutedContext.Exception);
			base.OnException(actionExecutedContext);
		}
	}
}