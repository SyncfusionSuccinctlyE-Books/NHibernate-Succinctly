using System;
using System.Collections;
using System.Linq;
using System.Security.Principal;
using NHibernate.Cfg;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Intercept;
using NHibernate.Proxy;

namespace Succintly.Common
{
	public class AuditableListener : IFlushEntityEventListener, ISaveOrUpdateEventListener, IMergeEventListener
	{
		public AuditableListener()
		{
			this.CurrentDateTimeProvider = () => DateTime.UtcNow;
			this.CurrentIdentityProvider = () => WindowsIdentity.GetCurrent().Name;
		}

		public Func<DateTime> CurrentDateTimeProvider
		{
			get;
			set;
		}

		public Func<String> CurrentIdentityProvider
		{
			get;
			set;
		}

		protected void ExplicitUpdateCall(IAuditable trackable)
		{
			if (trackable == null)
			{
				return;
			}

			trackable.UpdatedAt = this.CurrentDateTimeProvider();
			trackable.UpdatedBy = this.CurrentIdentityProvider();

			if (trackable.CreatedAt == DateTime.MinValue)
			{
				trackable.CreatedAt = trackable.UpdatedAt;
				trackable.CreatedBy = trackable.UpdatedBy;
			}
		}

		protected Boolean HasDirtyProperties(FlushEntityEvent @event)
		{
			if ((@event.EntityEntry.RequiresDirtyCheck(@event.Entity) == false) || (@event.EntityEntry.ExistsInDatabase == false) || (@event.EntityEntry.LoadedState == null))
			{
				return (false);
			}

			Object[] currentState = @event.EntityEntry.Persister.GetPropertyValues(@event.Entity, @event.Session.EntityMode);
			Object[] loadedState = @event.EntityEntry.LoadedState;

			return (@event.EntityEntry.Persister.EntityMetamodel.Properties
				.Where((property, i) => (LazyPropertyInitializer.UnfetchedProperty.Equals(currentState[i]) == false) && (property.Type.IsDirty(loadedState[i], currentState[i], @event.Session) == true))
				.Any());
		}

		public void Unregister(Configuration cfg)
		{
			cfg.EventListeners.SaveEventListeners = cfg.EventListeners.SaveEventListeners.Except(new ISaveOrUpdateEventListener[] { this }).ToArray();
			cfg.EventListeners.SaveOrUpdateEventListeners = cfg.EventListeners.SaveOrUpdateEventListeners.Except(new ISaveOrUpdateEventListener[] { this }).ToArray();
			cfg.EventListeners.UpdateEventListeners = cfg.EventListeners.UpdateEventListeners.Except(new ISaveOrUpdateEventListener[] { this }).ToArray();
			cfg.EventListeners.FlushEntityEventListeners = cfg.EventListeners.FlushEntityEventListeners.Except(new IFlushEntityEventListener[] { this }).ToArray();
			cfg.EventListeners.MergeEventListeners = cfg.EventListeners.MergeEventListeners.Except(new IMergeEventListener[] { this }).ToArray();
		}

		public void Register(Configuration cfg)
		{
			cfg.EventListeners.SaveEventListeners = new ISaveOrUpdateEventListener[] { this }.Concat(cfg.EventListeners.SaveEventListeners).ToArray();
			cfg.EventListeners.SaveOrUpdateEventListeners = new ISaveOrUpdateEventListener[] { this }.Concat(cfg.EventListeners.SaveOrUpdateEventListeners).ToArray();
			cfg.EventListeners.UpdateEventListeners = new ISaveOrUpdateEventListener[] { this }.Concat(cfg.EventListeners.UpdateEventListeners).ToArray();
			cfg.EventListeners.FlushEntityEventListeners = new IFlushEntityEventListener[] { this }.Concat(cfg.EventListeners.FlushEntityEventListeners).ToArray();
			cfg.EventListeners.MergeEventListeners = new IMergeEventListener[] { this }.Concat(cfg.EventListeners.MergeEventListeners).ToArray();
		}

		public void OnFlushEntity(FlushEntityEvent @event)
		{
			if ((@event.EntityEntry.Status == Status.Deleted) || (@event.EntityEntry.Status == Status.ReadOnly))
			{
				return;
			}

			IAuditable trackable = @event.Entity as IAuditable;
			
			if (trackable == null)
			{
				return;
			}

			if (this.HasDirtyProperties(@event) == true)
			{
				this.ExplicitUpdateCall(trackable);
			}
		}

		public void OnSaveOrUpdate(SaveOrUpdateEvent @event)
		{
			IAuditable auditable = @event.Entity as IAuditable;

			if (auditable.CreatedAt == DateTime.MinValue)
			{
				this.ExplicitUpdateCall(auditable);
			}
		}

		public void OnMerge(MergeEvent @event)
		{
			this.ExplicitUpdateCall(@event.Entity as IAuditable);
		}

		public void OnMerge(MergeEvent @event, IDictionary copiedAlready)
		{
			this.ExplicitUpdateCall(@event.Entity as IAuditable);
		}
	}
}
