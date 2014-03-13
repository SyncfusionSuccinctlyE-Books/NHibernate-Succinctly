using System;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System.Collections.Generic;

namespace Succinctly.Common
{
	public sealed class XmlConfiguration : BaseConfiguration
	{
		public override Configuration BuildConfiguration<TDriver, TDialect, TContext, TBatcherFactory>(BaseMapping mapping, String connectionStringName, Boolean updateSchema = false)
		{
			Configuration cfg = this.CreateConfiguration()
			.Configure();

			if (updateSchema == true)
			{
				SchemaExport export = new SchemaExport(cfg);
				export.Create(true, true);
			}

			return (cfg);
		}
	}
}
