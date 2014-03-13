using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Succinctly.Common;

namespace Succinctly.Model
{
	[Serializable]
	public class Record : IAuditable
	{
		public Record()
		{
			this.Children = new List<Record>();
		}

		public Int32 RecordId
		{
			get;
			set;
		}

		public Int32 Version
		{
			get;
			protected set;
		}

		public String Name
		{
			get;
			set;
		}

		public IList<Record> Children
		{
			get;
			protected set;
		}

		public Record Parent
		{
			get;
			set;
		}

		#region IAuditable Members

		public String CreatedBy
		{
			get;
			set;
		}

		public DateTime CreatedAt
		{
			get;
			set;
		}

		public String UpdatedBy
		{
			get;
			set;
		}

		public DateTime UpdatedAt
		{
			get;
			set;
		}

		#endregion

		public Boolean Deleted
		{
			get;
			set;
		}

		public override String ToString()
		{
			return (this.Name);
		}	
	}
}
