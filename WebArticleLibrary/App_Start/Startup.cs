using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebArticleLibrary.App_Start.Startup))]

namespace WebArticleLibrary.App_Start
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.MapSignalR();
		}
	}
}
