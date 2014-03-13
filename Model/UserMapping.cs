using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Succinctly.Model
{
	public class UserMapping : ClassMapping<User>
	{
		public UserMapping()
		{
			this.Table("user");
			this.Lazy(true);
			this.BatchSize(10);

			this.Id(x => x.UserId, x =>
			{
				x.Column("user_id");
				x.Generator(Generators.HighLow);
			});

			this.Property(x => x.Username, x =>
			{
				x.Column("username");
				x.Length(20);
				x.NotNullable(true);
			});
			this.Property(x => x.Birthday, x =>
			{
				x.Column("birthday");
				x.NotNullable(false);
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

			this.Set(x => x.Blogs, x =>
			{
				x.Key(y =>
				{
					y.Column("user_id");
					y.NotNullable(true);
				});
				x.Cascade(Cascade.All | Cascade.DeleteOrphans);
				x.Inverse(true);
				x.Lazy(CollectionLazy.Lazy);
				x.BatchSize(10);
			}, x =>
			{
				x.OneToMany();
			});
		}
	}
}
