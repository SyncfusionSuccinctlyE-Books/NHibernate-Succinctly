using NHibernate.Mapping.ByCode.Conformist;

namespace Succintly.Model
{
	//single table inheritance
	//public class NationalCitizenMappping : SubclassMapping<NationalCitizen>
	//table per class
	//public class NationalCitizenMappping : JoinedSubclassMapping<NationalCitizen>
	//table per concrete class
	public class NationalCitizenMappping : UnionSubclassMapping<NationalCitizen>
	//table per concrete class
	{
		public NationalCitizenMappping()
		{
			//single table inheritance
			//this.DiscriminatorValue("national_citizen");
			
			//table per class/table per concrete class
			this.Table("national_citizen");
			
			this.Lazy(true);

			//table per class
			/*this.Key(x =>
			{
			    x.Column("person_id");
			    x.NotNullable(true);
			});*/

			this.Property(x => x.NationalIdentityCard, x =>
			{
				x.Column("national_identity_card");
				x.Length(20);
				x.NotNullable(true);
			});
		}
	}
}
