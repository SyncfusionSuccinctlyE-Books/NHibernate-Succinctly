using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NHibernate;
using NHibernate.Linq;
using Succinctly.Model;
using System.ServiceModel;

namespace Succinctly.Web
{
	public partial class Default : Page
	{
		protected override void OnLoad(EventArgs e)
		{
			IProductService proxy = ChannelFactory<IProductService>.CreateChannel(new BasicHttpBinding(), new EndpointAddress("http://localhost:61530/ProductService.svc"));
			var products = proxy.GetProducts();

			Int32 moduleIndex = Array.IndexOf(this.Context.ApplicationInstance.Modules.AllKeys, typeof(NHibernateSessionModule).Name);
			NHibernateSessionModule module = this.Context.ApplicationInstance.Modules[moduleIndex] as NHibernateSessionModule;
		
			using (ISession session = module.SessionFactory.GetCurrentSession())
			{
				this.grid.DataSource = session.Query<Customer>().ToList();
				this.grid.DataBind();
			}

			base.OnLoad(e);
		}
	}
}