using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Mapping.Attributes;
using NHibernate.Cfg;
using System.Reflection;
using System.IO;

namespace Succinctly.Common
{
	public sealed class AttributeMapping : BaseMapping
	{
		public AttributeMapping() : this(Assembly.GetExecutingAssembly())
		{
		}

		public AttributeMapping(Assembly assembly)
		{
			this.Assembly = assembly;
		}

		public AttributeMapping(Type type) : this(type.Assembly)
		{
		}

		public AttributeMapping(String assemblyName)
		{
			this.Assembly = Assembly.Load(assemblyName);
		}

		public Assembly Assembly
		{
			get;
			private set;
		}

		public override Configuration Map(Configuration cfg)
		{
			HbmSerializer serializer = new HbmSerializer() { Validate = true };

			using (MemoryStream stream = serializer.Serialize(this.Assembly))
			{
				cfg.AddInputStream(stream);
			}

			return (cfg);
		}
	}
}
