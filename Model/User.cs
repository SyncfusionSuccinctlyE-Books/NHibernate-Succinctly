using System;
using System.Linq;
using System.Text;
using Iesi.Collections.Generic;

namespace Succinctly.Model
{
	[Serializable]
	[NHibernate.Mapping.Attributes.Class(Table = "user", Lazy = true, BatchSize = 10)]
	public class User
	{
		public User()
		{
			this.Blogs = new Iesi.Collections.Generic.HashedSet<Blog>();
			this.Details = new UserDetail();
		}

		[NHibernate.Mapping.Attributes.Id(0, Column = "user_id", Name = "UserId")]
		[NHibernate.Mapping.Attributes.Generator(1, Class = "hilo")]
		public virtual Int32 UserId
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Username", Column = "username", Length = 20, NotNull = true)]
		public virtual String Username
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.ComponentProperty(PropertyName = "Details")]
		public virtual UserDetail Details
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Birthday", Column = "birthday", NotNull = false)]
		public virtual DateTime? Birthday
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Set(0, Name = "Blogs", Cascade = "all-delete-orphan", Lazy = NHibernate.Mapping.Attributes.CollectionLazy.True, Inverse = true, BatchSize = 10, Generic = true)]
		[NHibernate.Mapping.Attributes.Key(1, Column = "user_id", NotNull = true)]
		[NHibernate.Mapping.Attributes.OneToMany(2, ClassType = typeof(Blog))]
		public virtual ISet<Blog> Blogs
		{
			get;
			protected set;
		}

		public override String ToString()
		{
			return (this.Details.Fullname);
		}
	}
}
