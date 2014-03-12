using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

namespace Succintly.Model
{	
	public class PersonMapping : ClassMapping<Person>
	{
		public PersonMapping()
		{
			this.Table("person");
			this.Lazy(true);

			this.Discriminator(x =>
			{
				x.Column("class");
				x.NotNullable(true);
			});

			this.Id(x => x.PersonId, x =>
			{
				x.Column("person_id");
				x.Generator(Generators.HighLow);
			});

			this.Property(x => x.Name, x =>
			{
				x.Column("name");
				x.NotNullable(true);
				x.Length(100);
			});

			this.Property(x => x.Gender, x =>
			{
				x.Column("gender");
				x.NotNullable(true);
				x.Type<EnumType<Gender>>();
			});
		}
	}
}