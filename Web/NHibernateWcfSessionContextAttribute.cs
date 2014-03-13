using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using NHibernate.Context;

namespace Succinctly.Web
{
	[Serializable]
	[AttributeUsage(AttributeTargets.Class,	AllowMultiple = false, Inherited = false)]
	public sealed class NHibernateWcfSessionContextAttribute : Attribute, IServiceBehavior, IDispatchMessageInspector
	{
		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
			{
				foreach (EndpointDispatcher endpoint in channelDispatcher.Endpoints)
				{
					endpoint.DispatchRuntime.MessageInspectors.Add(this);
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}

		public Object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
		{
			CurrentSessionContext.Bind(NHibernateSessionModule.Current.SessionFactory.OpenSession());
		  
			return (null);
		}

		public void BeforeSendReply(ref Message reply, Object correlationState)
		{
			var session = CurrentSessionContext.Unbind(NHibernateSessionModule.Current.SessionFactory);

			session.Dispose();
		}
	}
}