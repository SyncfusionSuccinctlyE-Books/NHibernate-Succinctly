using System;
using NHibernate.AdoNet;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Linq;

namespace Succintly.Common
{
	public abstract class BaseConfiguration
	{
		protected virtual Configuration CreateConfiguration()
		{
			Configuration cfg = new Configuration();

			return(cfg);
		}

		public Configuration BuildConfiguration(Type driverType, Type dialectType, Type contextType, Type batcherFactoryType, BaseMapping mapping, String connectionStringName, Boolean updateSchema = false)
		{
			if (driverType == null)
			{
				driverType = typeof(IDriver);
			}

			if (dialectType == null)
			{
				dialectType = typeof(Dialect);
			}

			if (contextType == null)
			{
				contextType = typeof(ICurrentSessionContext);
			}

			if (batcherFactoryType == null)
			{
				batcherFactoryType = typeof(NonBatchingBatcherFactory);
			}

			return (ReflectionHelper.GetMethod<BaseConfiguration>(x => x.BuildConfiguration<IDriver, Dialect, ICurrentSessionContext, IBatcherFactory>(null, null, false)).MakeGenericMethod(driverType, dialectType, contextType, batcherFactoryType).Invoke(this, new Object[] { mapping, connectionStringName, updateSchema }) as Configuration);
		}

		public Configuration BuildConfiguration<TDriver, TDialect>(BaseMapping mapping, String connectionStringName, Boolean updateSchema = false)
			where TDriver : IDriver
			where TDialect : Dialect
		{
			return (this.BuildConfiguration<TDriver, TDialect, ICurrentSessionContext, IBatcherFactory>(mapping, connectionStringName, updateSchema));
		}

		public Configuration BuildConfiguration<TDriver, TDialect, TContext>(BaseMapping mapping, String connectionStringName, Boolean updateSchema = false)
			where TDriver : IDriver
			where TDialect : Dialect
			where TContext : ICurrentSessionContext
		{
			return (this.BuildConfiguration<TDriver, TDialect, TContext, IBatcherFactory>(mapping, connectionStringName, updateSchema));
		}

		public abstract Configuration BuildConfiguration<TDriver, TDialect, TContext, TBatcherFactory>(BaseMapping mapping, String connectionStringName, Boolean updateSchema = false)
			where TDriver : IDriver
			where TDialect : Dialect
			where TContext : ICurrentSessionContext
			where TBatcherFactory : IBatcherFactory;
	}
}
