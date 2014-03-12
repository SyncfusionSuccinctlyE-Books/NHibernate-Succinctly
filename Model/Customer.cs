using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Succintly.Model
{
	[Serializable]
	public class Customer
	{
		public Customer()
		{
			this.RecentOrders = new HashSet<Order>();
			this.Orders = new HashSet<Order>();
		}

		[IsEven]
		public virtual Int32 CustomerId
		{
			get;
			set;
		}

		[NHibernate.Validator.Constraints.NotNullNotEmpty(Message = "The customer name is mandatory")]
		[NHibernate.Validator.Constraints.Length(Max = 50, Message = "The customer name can only have 50 characters")]
		public virtual String Name
		{
			get;
			set;
		}

		[NHibernate.Validator.Constraints.NotNullNotEmpty(Message = "The customer email is mandatory")]
		[NHibernate.Validator.Constraints.Email(Message = "The customer email must be a valid email adddress")]
		[NHibernate.Validator.Constraints.Length(Max = 50, Message = "The customer email can only have 50 characters")]
		public virtual String Email
		{
			get;
			set;
		}

		[NHibernate.Validator.Constraints.NotNull(Message = "The customer address is mandatory")]
		public virtual Address Address
		{
			get;
			set;
		}

		public virtual IEnumerable<Order> RecentOrders
		{
			get;
			protected set;
		}

		public virtual ICollection<Order> Orders
		{
			get;
			protected set;
		}

		public override String ToString()
		{
			return (this.Name);
		}

		public override Boolean Equals(Object obj)
		{
			if (obj as Customer == null)
			{
				return (false);
			}

			if (Object.ReferenceEquals(this, obj) == true)
			{
				return (true);
			}

			Customer other = obj as Customer;

			return((this.Name == other.Name) && (this.CustomerId == other.CustomerId) && (this.Address == other.Address));
		}
	}
}
