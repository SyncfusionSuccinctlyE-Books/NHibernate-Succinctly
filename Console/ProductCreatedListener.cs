using System;
using NHibernate.Event;
using Succintly.Model;

namespace Succintly.Console
{
	public class ProductCreatedListener : IFlushEntityEventListener
	{
		public static event Action<Product> ProductCreated;

		#region IFlushEntityEventListener Members

		public void OnFlushEntity(FlushEntityEvent @event)
		{
			if (@event.Entity is Product)
			{
				if (ProductCreated != null)
				{
					ProductCreated(@event.Entity as Product);
				}
			}
		}

		#endregion
	}
}
