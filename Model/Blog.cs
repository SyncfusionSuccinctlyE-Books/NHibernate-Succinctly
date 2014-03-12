using System;
using NHibernate.Classic;
using NHibernate;
using System.Collections.Generic;
using NHibernate.Type;
using System.Drawing;
using Succintly.Common;

namespace Succintly.Model
{
	[Serializable]
	[NHibernate.Mapping.Attributes.Class(Table = "blog", Lazy = true)]
	public class Blog : ILifecycle
	{
		public Blog()
		{
			this.Posts = new List<Post>();
		}

		[NHibernate.Mapping.Attributes.Id(0, Column = "blog_id", Name = "BlogId")]
		[NHibernate.Mapping.Attributes.Generator(1, Class = "hilo")]
		public virtual Int32 BlogId
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Picture", Column = "picture", NotNull = false, TypeType = typeof(ImageUserType), Lazy = true)]
		public virtual Image Picture
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "PostCount", Formula = "(SELECT COUNT(1) FROM post WHERE post.blog_id = blog_id)")]
		public virtual Int64 PostCount
		{
			get;
			protected set;
		}

		[NHibernate.Mapping.Attributes.ManyToOne(0, Column = "user_id", NotNull = true, Lazy = NHibernate.Mapping.Attributes.Laziness.NoProxy, Name = "Owner", Cascade = "save-update")]
		[NHibernate.Mapping.Attributes.Key(1)]
		public virtual User Owner
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Name", Column = "name", NotNull = true, Length = 50)]
		public virtual String Name
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Creation", Column = "creation", NotNull = true)]
		public virtual DateTime Creation
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.List(0, Name = "Posts", Cascade = "all-delete-orphan", Lazy = NHibernate.Mapping.Attributes.CollectionLazy.True, Inverse = true, Generic = true)]
		[NHibernate.Mapping.Attributes.Key(1, Column = "blog_id", NotNull = true)]
		[NHibernate.Mapping.Attributes.Index(2, Column = "number")]
		[NHibernate.Mapping.Attributes.OneToMany(3, ClassType = typeof(Post))]
		public virtual IList<Post> Posts
		{
			get;
			protected set;
		}

		public override String ToString()
		{
			return (this.Name);
		}

		#region ILifecycle Members

		LifecycleVeto ILifecycle.OnDelete(ISession session)
		{
			return (LifecycleVeto.NoVeto);
		}

		void ILifecycle.OnLoad(ISession session, Object id)
		{
		}

		LifecycleVeto ILifecycle.OnSave(ISession session)
		{
			return (LifecycleVeto.NoVeto);			
		}

		LifecycleVeto ILifecycle.OnUpdate(ISession session)
		{
			return (LifecycleVeto.NoVeto);
		}

		#endregion
	}
}
