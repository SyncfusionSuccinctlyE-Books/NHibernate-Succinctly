using System;
using NHibernate.Cfg;
using System.Reflection;

namespace Succintly.Common
{
	public sealed class XmlMapping : BaseMapping
	{
		public XmlMapping() : this(Assembly.GetCallingAssembly().FullName)
		{
		}

		public XmlMapping(String assemblyName)
		{
			this.AssemblyName = assemblyName;
		}

		public XmlMapping(Assembly assembly) : this(assembly.FullName)
		{
		}

		public XmlMapping(Type type) : this(type.Assembly)
		{
		}

		public String AssemblyName
		{
			get;
			private set;
		}

		public override Configuration Map(Configuration cfg)
		{
			cfg.AddAssembly(this.AssemblyName).Configure();

			return (cfg);
		}
	}
}
