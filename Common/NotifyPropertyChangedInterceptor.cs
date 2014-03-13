using System;
using System.Linq;
using System.ComponentModel;
using NHibernate;
using NHibernate.Proxy.DynamicProxy;
using System.Reflection;

namespace Succinctly.Common
{
	public sealed class NotifyPropertyChangedInterceptor : EmptyInterceptor
	{
		class _NotifyPropertyChangedInterceptor : NHibernate.Proxy.DynamicProxy.IInterceptor
		{
			private PropertyChangedEventHandler changed = delegate { };
			private readonly Object target = null;

			public _NotifyPropertyChangedInterceptor(Object target)
			{
				this.target = target;
			}

			#region IInterceptor Members

			public Object Intercept(InvocationInfo info)
			{
				Object result = null;

				if (info.TargetMethod.Name == "add_PropertyChanged")
				{
					PropertyChangedEventHandler propertyChangedEventHandler = info.Arguments[0] as PropertyChangedEventHandler;
					this.changed += propertyChangedEventHandler;
				}
				else if (info.TargetMethod.Name == "remove_PropertyChanged")
				{
					PropertyChangedEventHandler propertyChangedEventHandler = info.Arguments[0] as PropertyChangedEventHandler;
					this.changed -= propertyChangedEventHandler;
				}
				else
				{
					result = info.TargetMethod.Invoke(this.target, info.Arguments);
				}

				if (info.TargetMethod.Name.StartsWith("set_") == true)
				{
					String propertyName = info.TargetMethod.Name.Substring("set_".Length);
					this.changed(info.Target, new PropertyChangedEventArgs(propertyName));
				}

				return (result);
			}

			#endregion
		}

		private ISession session = null;

		private static readonly ProxyFactory factory = new ProxyFactory();

		public override void SetSession(ISession session)
		{
			this.session = session;
			base.SetSession(session);
		}

		public override Object Instantiate(String clazz, EntityMode entityMode, Object id)
		{
			Type entityType = this.session.SessionFactory.GetClassMetadata(clazz).GetMappedClass(entityMode);
			Object target = this.session.SessionFactory.GetClassMetadata(entityType).Instantiate(id, entityMode);
			Object proxy = factory.CreateProxy(entityType, new _NotifyPropertyChangedInterceptor(target), typeof(INotifyPropertyChanged));

			this.session.SessionFactory.GetClassMetadata(entityType).SetIdentifier(proxy, id, entityMode);

			return (proxy);
		}
	}
}
