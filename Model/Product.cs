using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Iesi.Collections.Generic;
using System.Drawing;

namespace Succintly.Model
{
	[Serializable]
	public class Product
	{
		public Product()
		{
			this.OrderDetails = new HashedSet<OrderDetail>();
			this.Attributes = new Dictionary<String, String>();
		}

		public virtual XDocument Specification
		{
			get;
			set;
		}

		public virtual Int32 ProductId
		{
			get;
			protected set;
		}

		public virtual String Name
		{
			get;
			set;
		}

		public virtual Int32 OrderCount
		{
			get;
			protected set;
		}

		public virtual Decimal Price
		{
			get;
			set;
		}

		//public virtual Byte[] Picture
		public virtual Image Picture
		{
			get;
			set;
		}

		public virtual IEnumerable<OrderDetail> OrderDetails
		{
			get;
			protected set;
		}

		public virtual IDictionary<String, String> Attributes
		{
			get;
			protected set;
		}

		public override String ToString()
		{
			return (this.Name);
		}
	}
}
