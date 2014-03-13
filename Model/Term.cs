using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succinctly.Model
{
	[Serializable]
	public class Term
	{
		public Term()
		{
			this.Translations = new Iesi.Collections.Generic.HashedSet<Translation>();
		}

		public virtual Int32 TermId
		{
			get;
			set;
		}

		public virtual String Description
		{
			get;
			set;
		}

		public virtual Iesi.Collections.Generic.ISet<Translation> Translations
		{
			get;
			protected set;
		}
	}
}
