using System;
using NHibernate.Cfg;
using System.Text;

namespace Succintly.Common
{
	public abstract class BaseMapping
	{
		protected String GetNormalizedDbName(String name)
		{
			StringBuilder builder = new StringBuilder();
			Boolean previousUpper = true;

			for (Int32 i = 0; i < name.Length; ++i)
			{
				Char c = name[i];

				if ((Char.IsLower(c) == false) && (previousUpper == false))
				{
					builder.Append('_');
				}

				previousUpper = (Char.IsLower(c) == false);

				builder.Append(c);
			}

			return (builder.ToString());
		}

		public abstract Configuration Map(Configuration cfg);
	}
}
