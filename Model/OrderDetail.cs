using System;
using Iesi.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succinctly.Model
{
	[Serializable]
	public class OrderDetail
	{
		public virtual Int32 OrderDetailId
		{
			get;
			protected set;
		}

		public virtual Order Order
		{
			get;
			set;
		}

		public virtual Product Product
		{
			get;
			set;
		}

		public virtual Int32 Quantity
		{
			get;
			set;
		}

		public virtual Decimal ItemsPrice
		{
			get
			{
				return (this.Quantity * (this.Product != null ? this.Product.Price : 0));
			}
		}

		public override String ToString()
		{
			return (String.Concat(this.Product, " * ", this.Quantity));
		}
	}
}
