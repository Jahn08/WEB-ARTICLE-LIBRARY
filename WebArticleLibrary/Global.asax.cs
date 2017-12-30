using System.Web;
using System.Web.Http;

namespace WebArticleLibrary
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
			GlobalConfiguration.Configuration.Filters.Add(new App_Start.HandleErrorAttribute());

			HttpContext.Current.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Required);

			Model.Migrations.Configuration.UpdateDatabase();
		}
	}
}
