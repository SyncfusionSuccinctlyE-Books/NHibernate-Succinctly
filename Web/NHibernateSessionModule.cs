using System;
using System.Web;
using NHibernate;
using NHibernate.Context;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using Succintly.Model;
using NHibernate.Type;
using NHibernate.Cfg.MappingSchema;
using Succintly.Common;
using NHibernate.Driver;
using NHibernate.Dialect;
using NHibernate.AdoNet;

namespace Succintly.Web
{
	public class NHibernateSessionModule : IHttpModule
	{
		public ISessionFactory SessionFactory
		{
			get;
			private set;
		}

		public static NHibernateSessionModule Current
		{
			get
			{
				return (HttpContext.Current.ApplicationInstance.Modules[Array.IndexOf(HttpContext.Current.ApplicationInstance.Modules.AllKeys, typeof(NHibernateSessionModule).Name)] as NHibernateSessionModule);
			}
		}

		#region IHttpModule Members

		public void Dispose()
		{
			this.SessionFactory.Dispose();
		}

		public void Init(HttpApplication context)
		{
			BaseConfiguration config = new LoquaciousConfiguration();
			Configuration cfg = config
			.BuildConfiguration<Sql2008ClientDriver, MsSql2008Dialect, WebSessionContext, SqlClientBatchingBatcherFactory>(new ExplicitMapping(), "Succintly", true);

			this.SessionFactory = cfg.BuildSessionFactory();

			context.BeginRequest += this.OnBeginRequest;
			context.EndRequest += this.OnEndRequest;
			context.Error += this.OnError;
		}

		#endregion

		protected void OnBeginRequest(Object sender, EventArgs e)
		{
			ISession session = this.SessionFactory.OpenSession();
			session.BeginTransaction();
			
			CurrentSessionContext.Bind(session);
		}

		protected void OnEndRequest(Object sender, EventArgs e)
		{
			this.DisposeOfSession(true);
		}

		protected void OnError(Object sender, EventArgs e)
		{
			this.DisposeOfSession(false);
		}

		protected void DisposeOfSession(Boolean commit)
		{
			ISession session = CurrentSessionContext.Unbind(this.SessionFactory);

			if (session != null)
			{
				if ((session.Transaction.IsActive == true) && (session.Transaction.WasCommitted == false) && (session.Transaction.WasRolledBack == false))
				{
					if (commit == true)
					{
						session.Transaction.Commit();
					}
					else
					{
						session.Transaction.Rollback();
					}

					session.Transaction.Dispose();
				}

				session.Dispose();
			}
		}
	}
}