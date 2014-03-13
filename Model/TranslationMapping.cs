using NHibernate.Mapping.ByCode.Conformist;

namespace Succinctly.Model
{
	public class TranslationMapping : ClassMapping<Translation>
	{
		public TranslationMapping()
		{
			this.Table("translation");
			this.Lazy(true);
			//this.Filter("CurrentLanguage", x =>
			//{
			//    x.Condition("language_id = :code");
			//});

			this.ComponentAsId(x => x.TranslationId, x =>
			{
				x.ManyToOne(y => y.Language, y =>
				{
					y.Column("language_id");
				});
				x.ManyToOne(y => y.Term, y =>
				{
					y.Column("term_id");
				});
			});

			this.Property(x => x.Text, x =>
			{
				x.Column("text");
				x.Length(100);
				x.NotNullable(true);
			});
		}
	}
}