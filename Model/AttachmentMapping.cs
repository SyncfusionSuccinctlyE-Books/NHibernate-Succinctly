using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

namespace Succinctly.Model
{
	public class AttachmentMapping : ClassMapping<Attachment>
	{
		public AttachmentMapping()
		{
			this.Table("attachment");
			this.Lazy(true);

			this.Id(x => x.AttachmentId, x =>
			{
				x.Column("attachment_id");
				x.Generator(Generators.HighLow);
			});

			this.Property(x => x.Filename, x =>
			{
				x.Column("filename");
				x.Length(50);
				x.NotNullable(true);
			});
			this.Property(x => x.Timestamp, x =>
			{
				x.Column("timestamp");
				x.NotNullable(true);
			});
			this.Property(x => x.Contents, x =>
			{
				x.Column("contents");
				x.Length(100000);
				x.Type<BinaryBlobType>();
				x.NotNullable(true);
				x.Lazy(true);
			});

			this.ManyToOne(x => x.Post, x =>
			{
				x.Column("post_id");
				x.Lazy(LazyRelation.NoProxy);
				x.NotNullable(true);
			});
		}
	}
}
