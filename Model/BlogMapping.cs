using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Succinctly.Common;
using System.Drawing.Imaging;

namespace Succinctly.Model
{
	public class BlogMapping : ClassMapping<Blog>
	{
		public BlogMapping()
		{
			this.Table("blog");
			this.Lazy(true);
		
			this.Id(x => x.BlogId, x =>
			{
				x.Column("blog_id");
				x.Generator(Generators.HighLow);
			});

			this.Property(x => x.Name, x =>
			{
				x.Column("name");
				x.Length(50);
				x.NotNullable(true);
			});
			this.Property(x => x.Picture, x =>
			{
				x.Column("picture");
				x.NotNullable(false);
				x.Type<ImageUserType>(new { ImageFormat = ImageFormat.Gif });
			});
			this.Property(x => x.Creation, x =>
			{
				x.Column("creation");
				x.NotNullable(true);
			});
			this.Property(x => x.PostCount, x =>
			{
				x.Formula("(SELECT COUNT(1) FROM post WHERE post.blog_id = blog_id)");
			});

			this.ManyToOne(x => x.Owner, x =>
			{				
				x.Cascade(Cascade.Persist);
				x.Column("user_id");
				x.NotNullable(true);
				x.Lazy(LazyRelation.NoProxy);
			});

			this.List(x => x.Posts, x =>
			{
				x.Key(y =>
				{
					y.Column("blog_id");
					y.NotNullable(true);
				});
				x.Index(y =>
				{
					y.Column("number");
				});
				x.Lazy(CollectionLazy.Lazy);
				x.Cascade(Cascade.All | Cascade.DeleteOrphans);
				x.Inverse(true);
			}, x =>
			{
				x.OneToMany();
			});
		}
	}
}
