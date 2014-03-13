using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Succinctly.Model;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;

namespace Succinctly.Model
{
	public class TermMapping : ClassMapping<Term>
	{
		public TermMapping()
		{
			this.Table("term");
			this.Lazy(true);

			this.Id(x => x.TermId, x =>
			{
				x.Column("term_id");
				x.Generator(Generators.HighLow);
			});

			this.NaturalId(x =>
			{
				x.Property(y => y.Description, y =>
				{
					y.Column("description");
					y.NotNullable(true);
					y.Length(50);
				});
			});

			this.Set(x => x.Translations, x =>
			{
				x.Key(y =>
				{
					y.Column("term_id");
					y.NotNullable(true);
				});
				//x.Filter("", z =>
				//{
				//    z.Condition("language_id = :code");
				//});
				x.Inverse(true);
				x.Cascade(Cascade.All | Cascade.DeleteOrphans);
				x.Lazy(CollectionLazy.Lazy);
			}, x =>
			{
				x.OneToMany();
			});
		}
	}
}