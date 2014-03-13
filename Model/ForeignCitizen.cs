using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succinctly.Model
{
	[Serializable]
	//[NHibernate.Mapping.Attributes.Subclass(DiscriminatorValue = "foreign_citizen", ExtendsType = typeof(Person), Lazy = true)]
	//[NHibernate.Mapping.Attributes.JoinedSubclass(0, Table = "foreign_citizen", ExtendsType = typeof(Person), Lazy = true)]
	//[NHibernate.Mapping.Attributes.Key(1, Column = "person_id")]
	[NHibernate.Mapping.Attributes.UnionSubclass(0, Table = "foreign_citizen", ExtendsType = typeof(Person), Lazy = true)]
	public class ForeignCitizen : Person
	{
		[NHibernate.Mapping.Attributes.Property(Name = "Passport", Column = "passport", Length = 50, NotNull = false)]
		public virtual String Passport
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Country", Column = "country", Length = 50, NotNull = false)]
		public virtual String Country
		{
			get;
			set;
		}
	}
}
