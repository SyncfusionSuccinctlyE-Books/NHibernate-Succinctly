using System;
using NHibernate.Cfg;
using NHibernate.Driver;
using NHibernate.Dialect;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Context;
using NHibernate.AdoNet;

namespace Succintly.Common
{
	public sealed class LoquaciousConfiguration : BaseConfiguration
	{
		public override Configuration BuildConfiguration<TDriver, TDialect, TContext, TBatcherFactory>(BaseMapping mapping, String connectionStringName, Boolean updateSchema = false)
		{
			Configuration cfg = this.CreateConfiguration()
			.DataBaseIntegration(db =>
			{
				db.ConnectionStringName = connectionStringName;
				db.Dialect<TDialect>();
				db.Driver<TDriver>();
				db.HqlToSqlSubstitutions = "true 1, false 0, yes 'Y', no 'N'";
				db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
				db.SchemaAction = (updateSchema == true) ? SchemaAutoAction.Update : SchemaAutoAction.Validate;
				db.LogFormattedSql = true;
				db.BatchSize = 100;

				if (typeof(TBatcherFactory) != typeof(IBatcherFactory))
				{
					db.Batcher<TBatcherFactory>();
				}
			})
			.SetProperty(NHibernate.Cfg.Environment.WrapResultSets, Boolean.TrueString)
			.SetProperty(NHibernate.Cfg.Environment.GenerateStatistics, Boolean.FalseString)
			.SetProperty(NHibernate.Cfg.Environment.QueryStartupChecking, Boolean.TrueString)
			.SetProperty(NHibernate.Cfg.Environment.PrepareSql, Boolean.TrueString);

			if (typeof(TContext) != typeof(ICurrentSessionContext))
			{
				cfg = cfg.CurrentSessionContext<TContext>();
			}

			cfg = mapping.Map(cfg);

			return (cfg);
		}
	}
}
