using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succintly.Model
{
	[Serializable]
	public class Language
	{
		public virtual String LanguageId
		{
			get;
			set;
		}

		public virtual String Name
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
