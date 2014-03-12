using NHibernate.Mapping.ByCode.Conformist;

namespace Succintly.Model
{
	//single table inheritance
	//public class ForeignCitizenMapping : SubclassMapping<ForeignCitizen>
	//table per class
	//public class ForeignCitizenMapping : JoinedSubclassMapping<ForeignCitizen>
	//table per concrete class
	public class ForeignCitizenMapping : UnionSubclassMapping<ForeignCitizen>
	{
		public ForeignCitizenMapping()
		{
			//single table inheritance
			//this.DiscriminatorValue("foreign_citizen");

			//table per class/table per concrete class
			this.Table("foreign_citizen");
			
			this.Lazy(true);

			//table per class/
			/*this.Key(x =>
			{
				x.Column("person_id");
				x.NotNullable(true);
			});*/

			this.Property(x => x.Country, x =>
			{
				x.Column("country");
				x.Length(20);
				x.NotNullable(true);
			});

			this.Property(x => x.Passport, x =>
			{
				x.Column("passport");
				x.Length(20);
				x.NotNullable(true);
			});
		}
	}
}
