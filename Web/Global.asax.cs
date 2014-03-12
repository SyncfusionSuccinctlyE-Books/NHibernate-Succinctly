using System;
using System.Web;
using log4net.Config;

namespace Succintly.Web
{
	public class Global : HttpApplication
	{
		protected void Application_Start()
		{
			XmlConfigurator.Configure();
		}
	}
}