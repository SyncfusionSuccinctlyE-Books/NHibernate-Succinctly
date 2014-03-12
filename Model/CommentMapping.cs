using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Succintly.Model
{
	public class CommentMapping : ClassMapping<Comment>
	{
		public CommentMapping()
		{
			this.Table("comment");
			this.Lazy(true);

			this.Id(x => x.CommentId, x =>
			{
				x.Column("comment_id");
				x.Generator(Generators.HighLow);
			});

			this.Property(x => x.Content, x =>
			{
				x.Column("content");
				x.NotNullable(true);
				x.Length(2000);
				x.Lazy(true);
				x.Type(NHibernateUtil.StringClob);
			});
			this.Property(x => x.Timestamp, x =>
			{
				x.Column("timestamp");
				x.NotNullable(true);
			});

			this.Component(x => x.Details, x =>
			{
				x.Property(y => y.Fullname, z =>
				{
					z.Column("fullname");
					z.Length(50);
					z.NotNullable(true);
				});
				x.Property(y => y.Email, z =>
				{
					z.Column("email");
					z.Length(50);
					z.NotNullable(true);
				});
				x.Property(y => y.Url, z =>
				{
					z.Column("url");
					z.Length(50);
					z.NotNullable(false);
				});
			});

			this.ManyToOne(x => x.Post, x =>
			{
				x.Column("post_id");
				x.NotNullable(true);
				x.Lazy(LazyRelation.NoProxy);
			});
		}
	}
}
