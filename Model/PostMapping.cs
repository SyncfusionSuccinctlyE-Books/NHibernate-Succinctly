using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Succinctly.Model
{
	public class PostMapping : ClassMapping<Post>
	{
		public PostMapping()
		{
			this.Table("post");
			this.Lazy(true);

			this.Id(x => x.PostId, x =>
			{
				x.Column("post_id");
				x.Generator(Generators.HighLow);
			});

			this.Property(x => x.Title, x =>
			{
				x.Column("title");
				x.Length(50);
				x.NotNullable(true);
			});
			this.Property(x => x.Timestamp, x =>
			{
				x.Column("timestamp");
				x.NotNullable(true);
			});
			this.Property(x => x.Content, x =>
			{
				x.Column("content");
				x.Length(2000);
				x.NotNullable(true);
				x.Type(NHibernateUtil.StringClob);
			});

			this.ManyToOne(x => x.Blog, x =>
			{
				x.Column("blog_id");
				x.NotNullable(true);
				x.Lazy(LazyRelation.NoProxy);
			});

			this.Set(x => x.Tags, x =>
			{
				x.Key(y =>
				{
					y.Column("post_id");
					y.NotNullable(true);
				});
				x.Cascade(Cascade.All);
				x.Lazy(CollectionLazy.NoLazy);
				x.Table("tag");
				x.OrderBy("tag");
			}, x =>
			{
				x.Element(y =>
				{
					y.Column("tag");
					y.Length(20);
					y.NotNullable(true);
					y.Unique(true);
				});
			});
			this.Set(x => x.Attachments, x =>
			{
				x.Key(y =>
				{
					y.Column("post_id");
					y.NotNullable(true);
				});
				x.Cascade(Cascade.All | Cascade.DeleteOrphans);
				x.Lazy(CollectionLazy.Lazy);
				x.Inverse(true);
			}, x =>
			{
				x.OneToMany();
			});
			this.Bag(x => x.Comments, x =>
			{
				x.Key(y =>
				{
					y.Column("post_id");
				});
				x.Cascade(Cascade.All | Cascade.DeleteOrphans);
				x.Lazy(CollectionLazy.Lazy);
				x.Inverse(true);
			}, x =>
			{
				x.OneToMany();
			});
		}
	}
}
