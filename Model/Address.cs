using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succintly.Model
{
	[Serializable]
	public class Address
	{
		public virtual Int32 CustomerId
		{
			get;
			set;
		}

		public virtual String City
		{
			get;
			set;
		}

		public virtual String Country
		{
			get;
			set;
		}

		public virtual String Street
		{
			get;
			set;
		}

		public virtual String ZipCode
		{
			get;
			set;
		}

		public virtual Customer Customer
		{
			get;
			set;
		}
	}
}
