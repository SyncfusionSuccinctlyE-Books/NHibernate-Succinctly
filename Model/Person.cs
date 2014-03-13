using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Type;

namespace Succinctly.Model
{
	[Serializable]
	[NHibernate.Mapping.Attributes.Class(0, Table = "person", Lazy = true, Abstract = true)]
	//[NHibernate.Mapping.Attributes.Discriminator(1, Column = "class")]
	public abstract class Person
	{
		[NHibernate.Mapping.Attributes.Id(0, Name = "PersonId", Column = "person_id")]
		[NHibernate.Mapping.Attributes.Generator(1, Class = "hilo")]
		public virtual Int32 PersonId
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Name", Column = "name", Length = 100, NotNull = true)]
		public virtual String Name
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Gender", Column = "gender", TypeType = typeof(EnumType<Gender>), NotNull = true)]
		public virtual Gender Gender
		{
			get;
			set;
		}
	}
}
