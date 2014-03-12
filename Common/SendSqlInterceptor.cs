using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.SqlCommand;

namespace Succintly.Common
{
	public class SendSqlInterceptor : EmptyInterceptor
	{
		private readonly Func<String> sqlBefore = null;
		private readonly Func<String> sqlAfter = null;

		public SendSqlInterceptor(Func<String> sqlBefore, Func<String> sqlAfter = null)
		{
			this.sqlBefore = sqlBefore;
			this.sqlAfter = sqlAfter;
		}

		public override SqlString OnPrepareStatement(SqlString sql)
		{
			sql = sql.Insert(0, String.Format("{0};", this.sqlBefore()));

			if (this.sqlAfter != null)
			{
				sql = sql.Append(String.Format(";{0}", this.sqlAfter()));
			}

			return (base.OnPrepareStatement(sql));
		}
	}
}
