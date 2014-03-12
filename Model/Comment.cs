using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Type;

namespace Succintly.Model
{
	[Serializable]
	[NHibernate.Mapping.Attributes.Class(Table = "comment", Lazy = true)]
	public class Comment
	{
		public Comment()
		{
			this.Details = new UserDetail();
		}

		[NHibernate.Mapping.Attributes.Id(0, Column = "comment_id", Name = "CommentId")]
		[NHibernate.Mapping.Attributes.Generator(1, Class = "hilo")]
		public virtual Int32 CommentId
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

		[NHibernate.Mapping.Attributes.Property(Name = "Timestamp", Column = "timestamp", NotNull = true)]
		public virtual DateTime Timestamp
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Content", Column = "content", NotNull = true, Length = 2000, Lazy = true, Type = "StringClob")]
		public virtual String Content
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.ManyToOne(0, Column = "post_id", NotNull = true, Lazy = NHibernate.Mapping.Attributes.Laziness.NoProxy, Name = "Post")]
		[NHibernate.Mapping.Attributes.Key(1)]
		public virtual Post Post
		{
			get;
			set;
		}

		public override String ToString()
		{
			return (this.Content);
		}
	}
}
