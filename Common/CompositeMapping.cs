using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Cfg;

namespace Succintly.Common
{
	public sealed class CompositeMapping : BaseMapping
	{
		public CompositeMapping(params BaseMapping [] mappings)
		{
			this.Mappings = mappings;
		}

		public BaseMapping[] Mappings
		{
			get;
			private set;
		}

		public override Configuration Map(Configuration cfg)
		{
			foreach (BaseMapping mapping in this.Mappings)
			{
				cfg = mapping.Map(cfg);
			}

			return (cfg);
		}
	}
}
