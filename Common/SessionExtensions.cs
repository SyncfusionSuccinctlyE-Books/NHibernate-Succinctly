using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Intercept;
using NHibernate.Persister.Entity;
using NHibernate.Tuple;
using NHibernate.Proxy;

namespace Succinctly.Common
{
	public static class SessionExtensions
	{
		public static T Attach<T>(this ISession session, T entity, LockMode mode = null)
		{
			mode = mode ?? LockMode.None;

			IEntityPersister persister = session.GetSessionImplementation().GetEntityPersister(NHibernateProxyHelper.GuessClass(entity).FullName, entity);
			Object[] fields = persister.GetPropertyValues(entity, session.ActiveEntityMode);
			Object id = persister.GetIdentifier(entity, session.ActiveEntityMode);
			Object version = persister.GetVersion(entity, session.ActiveEntityMode);
			EntityEntry entry = session.GetSessionImplementation().PersistenceContext.AddEntry(entity, Status.Loaded, fields, null, id, version, LockMode.None, true, persister, true, false);
			
			return (entity);
		}

		public static Dictionary<String, Object> GetDirtyProperties<T>(this ISession session, T entity)
		{
			ISessionImplementor sessionImpl = session.GetSessionImplementation();
			IPersistenceContext context = sessionImpl.PersistenceContext;
			EntityEntry entry = context.GetEntry(context.Unproxy(entity));

			if ((entry == null) || (entry.RequiresDirtyCheck(entity) == false) || (entry.ExistsInDatabase == false) || (entry.LoadedState == null))
			{
				return (null);
			}

			IEntityPersister persister = entry.Persister;
			String[] propertyNames = persister.PropertyNames;
			Object[] currentState = persister.GetPropertyValues(entity, sessionImpl.EntityMode);
			Object[] loadedState = entry.LoadedState;
			IEnumerable<StandardProperty> dp = (persister.EntityMetamodel.Properties.Where((property, i) => (LazyPropertyInitializer.UnfetchedProperty.Equals(loadedState[i]) == false) && (property.Type.IsDirty(loadedState[i], currentState[i], sessionImpl) == true))).ToArray();

			return (dp.ToDictionary(x => x.Name, x => currentState[Array.IndexOf(propertyNames, x.Name)]));
		}

		public static IEnumerable<T> Local<T>(this ISession session, Status status = Status.Loaded)
		{
			ISessionImplementor impl = session.GetSessionImplementation();
			IPersistenceContext pc = impl.PersistenceContext;

			foreach (T key in pc.EntityEntries.Keys.OfType<T>())
			{
				EntityEntry entry = pc.EntityEntries[key] as EntityEntry;

				if (entry.Status == status)
				{
					yield return (key);
				}
			}
		}

		public static EntityEntry Entry<T>(this ISession session, T entity)
		{
			ISessionImplementor impl = session.GetSessionImplementation();
			IPersistenceContext pc = impl.PersistenceContext;			
			EntityEntry entry = pc.EntityEntries[entity] as EntityEntry;

			return (entry);
		}
	}
}
