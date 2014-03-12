using System;
using System.Linq;
using System.Text;
using Iesi.Collections.Generic;
using System.Collections.Generic;
using NHibernate.Type;

namespace Succintly.Model
{
	[Serializable]
	[NHibernate.Mapping.Attributes.Class(Table = "post", Lazy = true)]
	public class Post
	{
		public Post()
		{
			this.Tags = new Iesi.Collections.Generic.HashedSet<String>();
			this.Attachments = new Iesi.Collections.Generic.HashedSet<Attachment>();
			this.Comments = new List<Comment>();
		}

		[NHibernate.Mapping.Attributes.Id(0, Column = "post_id", Name = "PostId")]
		[NHibernate.Mapping.Attributes.Generator(1, Class = "hilo")]
		public virtual Int32 PostId
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.ManyToOne(0, Column = "blog_id", NotNull = true, Lazy = NHibernate.Mapping.Attributes.Laziness.NoProxy, Name = "Blog")]
		[NHibernate.Mapping.Attributes.Key(1)]
		public virtual Blog Blog
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Timestamp", Column = "timestamp", NotNull = true)]
		public virtual DateTime Timestamp
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Title", Column = "title", Length = 50, NotNull = true)]
		public virtual String Title
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Content", Column = "content", Length = 2000, NotNull = true, Lazy = true, Type = "StringClob")]
		public virtual String Content
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Set(0, Name = "Tags", Table = "tag", OrderBy = "tag", Lazy = NHibernate.Mapping.Attributes.CollectionLazy.False, Cascade = "all", Generic = true)]
		[NHibernate.Mapping.Attributes.Key(1, Column = "post_id", Unique = true, NotNull = true)]
		[NHibernate.Mapping.Attributes.Element(2, Column = "tag", Length = 20, NotNull = true, Unique = true)]
		public virtual Iesi.Collections.Generic.ISet<String> Tags
		{
			get;
			protected set;
		}

		[NHibernate.Mapping.Attributes.Set(0, Name = "Attachments", Inverse = true, Lazy = NHibernate.Mapping.Attributes.CollectionLazy.True, Cascade = "all-delete-orphan", Generic = true)]
		[NHibernate.Mapping.Attributes.Key(1, Column = "post_id", NotNull = true)]
		[NHibernate.Mapping.Attributes.OneToMany(2, ClassType = typeof(Attachment))]
		public virtual Iesi.Collections.Generic.ISet<Attachment> Attachments
		{
			get;
			protected set;
		}

		[NHibernate.Mapping.Attributes.Bag(0, Name = "Comments", Inverse = true, Lazy = NHibernate.Mapping.Attributes.CollectionLazy.True, Cascade = "all-delete-orphan", Generic = true)]
		[NHibernate.Mapping.Attributes.Key(1, Column = "post_id", NotNull = true)]
		[NHibernate.Mapping.Attributes.OneToMany(2, ClassType = typeof(Comment))]
		public virtual IList<Comment> Comments
		{
			get;
			protected set;
		}

		public override String ToString()
		{
			return (this.Title);
		}
	}
}
