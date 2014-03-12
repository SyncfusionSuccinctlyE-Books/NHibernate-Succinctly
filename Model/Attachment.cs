using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succintly.Model
{
	[Serializable]
	[NHibernate.Mapping.Attributes.Class(Table = "attachment", Lazy = true)]
	public class Attachment
	{
		[NHibernate.Mapping.Attributes.Id(0, Column = "attachment_id", Name = "AttachmentId")]
		[NHibernate.Mapping.Attributes.Generator(1, Class = "hilo")]
		public virtual Int32 AttachmentId
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Filename", Column = "filename", Length = 50, NotNull = true)]
		public virtual String Filename
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Contents", Column = "contents", NotNull = true, Length = 100000, Type = "BinaryBlob")]
		public virtual Byte[] Contents
		{
			get;
			set;
		}

		[NHibernate.Mapping.Attributes.Property(Name = "Timestamp", Column = "timestamp", NotNull = true, OptimisticLock = false)]
		public virtual DateTime Timestamp
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
	}
}
