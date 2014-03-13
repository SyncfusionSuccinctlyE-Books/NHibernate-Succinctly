using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succinctly.Common
{
	public interface IAuditable
	{
		String CreatedBy
		{
			get;
			set;
		}

		DateTime CreatedAt
		{
			get;
			set;
		}

		String UpdatedBy
		{
			get;
			set;
		}

		DateTime UpdatedAt
		{
			get;
			set;
		}
	}
}
