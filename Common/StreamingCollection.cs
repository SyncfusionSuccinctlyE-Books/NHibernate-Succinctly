using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Succintly.Common
{
	public class StreamingCollection<T> : Collection<T>
	{
		private Action<T> action = null;

		public StreamingCollection(Action<T> action)
		{
			this.action = action;
		}

		public Boolean JustStream
		{
			get;
			set;
		}

		protected override void InsertItem(Int32 index, T item)
		{
			this.action(item);

			if (this.JustStream == false)
			{
				base.InsertItem(index, item);
			}
		}
	}
}
