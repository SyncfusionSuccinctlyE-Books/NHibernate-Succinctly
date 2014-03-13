using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succinctly.Model
{
	[Serializable]
	[NHibernate.Mapping.Attributes.Component]
	public class UserDetail
	{
		[NHibernate.Mapping.Attributes.Property(Name = "Url", Column = "url", Length = 50, NotNull = false)]
		public String Url
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Fullname", Column = "fullname", Length = 50, NotNull = true)]
		public String Fullname
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Email", Column = "email", Length = 50, NotNull = true)]
		public String Email
		{
			get;
			set;
		}

		public override String ToString()
		{
			return (String.Concat("Fullname=", this.Fullname, ", Email=", this.Email, ", Url=", this.Url));
		}
	}
}
