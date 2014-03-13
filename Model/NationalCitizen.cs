using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succinctly.Model
{
	[Serializable]
	//[NHibernate.Mapping.Attributes.Subclass(DiscriminatorValue = "national_citizen", ExtendsType = typeof(Person), Lazy = true)]
	//[NHibernate.Mapping.Attributes.JoinedSubclass(0, Table = "national_citizen", ExtendsType = typeof(Person), Lazy = true)]
	//[NHibernate.Mapping.Attributes.Key(1, Column = "person_id")]
	[NHibernate.Mapping.Attributes.UnionSubclass(0, Table = "national_citizen", ExtendsType = typeof(Person), Lazy = true)]
	public class NationalCitizen : Person
	{
		[NHibernate.Mapping.Attributes.Property(Name = "NationalIdentityCard", Column = "national_identity_card", Length = 50, NotNull = false)]
		public virtual String NationalIdentityCard
		{
			get;
			set;
		}
	}
}
