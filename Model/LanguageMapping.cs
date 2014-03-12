using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Succintly.Model;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;

namespace Succintly.Model
{
	public class LanguageMapping : ClassMapping<Language>
	{
		public LanguageMapping()
		{
			this.Table("language");
			this.Lazy(true);

			this.Id(x => x.LanguageId, x =>
			{
				x.Column("language_id");
				x.Generator(Generators.Assigned);
			});

			this.Property(x => x.Name, x =>
			{
				x.Column("name");
				x.NotNullable(true);
				x.Length(100);
			});
		}
	}
}