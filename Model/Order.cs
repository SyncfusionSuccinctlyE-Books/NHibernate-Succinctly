using System;
using Iesi.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Succintly.Model
{
	[Serializable]
	public class Order
	{
		public Order()
		{
			this.Details = new HashedSet<OrderDetail>();
		}

		public virtual Int32 OrderId
		{
			get;
			set;
		}

		public virtual OrderState State
		{
			get;
			set;
		}

		public virtual DateTime Date
		{
			get;
			set;
		}

		public virtual Customer Customer
		{
			get;
			set;
		}

		public virtual Iesi.Collections.Generic.ISet<OrderDetail> Details
		{
			get;
			set;
		}

		public override String ToString()
		{
			return (String.Concat(this.Customer, "@", this.Date));
		}
	}
}
