using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succintly.Model
{
	[Serializable]
	public enum OrderState
	{
		Received = 0,
		Processed = 1,
		Sent = 2
	}
}
