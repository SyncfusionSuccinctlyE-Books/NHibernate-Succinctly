using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Succintly.Model
{
	[Serializable]
	public sealed class LanguageTerm
	{
		public Language Language
		{
			get;
			set;
		}

		public Term Term
		{
			get;
			set;
		}

		public override Int32 GetHashCode()
		{
			Int32 result = 1;
			result = (result * 397) ^ this.Term.GetHashCode();
			result = (result * 397) ^ this.Language.GetHashCode();

			return (result);
		}
		public override Boolean Equals(Object obj)
		{
			if (obj as LanguageTerm == null)
			{
				return (false);
			}

			LanguageTerm other = obj as LanguageTerm;

			return ((Object.Equals(this.Language, other.Language)) && (Object.Equals(this.Term, other.Term)));
		}
	}

	[Serializable]
	public class Translation
	{	
		public virtual LanguageTerm TranslationId
		{
			get;
			set;
		}

		public virtual String Text
		{
			get;
			set;
		}

		public virtual String Code
		{
			get;
			protected set;
		}

		public override String ToString()
		{
			return (this.Text);
		}
	}
}
