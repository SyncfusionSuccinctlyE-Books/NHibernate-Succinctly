using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Succintly.Model;
using NHibernate;
using NHibernate.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Web;

namespace Succintly.Web
{
	[DataContract]
	public class ProductDTO
	{
		[DataMember]
		public String Name
		{
			get;
			set;
		}

		[DataMember]
		public Decimal Price
		{
			get;
			set;
		}

		[DataMember]
		public Int32 Orders
		{
			get;
			set;
		}
	}

	[ServiceContract]
	public interface IProductService
	{
		[OperationContract]
		IEnumerable<ProductDTO> GetProducts();
	}

	//[NHibernateWcfSessionContext]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class ProductService : IProductService
	{		
		public IEnumerable<ProductDTO> GetProducts()
		{
			using (ISession session = NHibernateSessionModule.Current.SessionFactory.GetCurrentSession())
			{
				var products = session.Query<Product>().Select(x => new ProductDTO { Name = x.Name, Price = x.Price, Orders = x.OrderCount }).ToList();
				return (products);
			}
		}
	}
}