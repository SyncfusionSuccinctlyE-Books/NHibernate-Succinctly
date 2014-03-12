using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Validator.Constraints;
using NHibernate.Validator.Engine;
using NHibernate.Mapping;

namespace Succintly.Model
{
	[Serializable]
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class IsEvenAttribute : EmbeddedRuleArgsAttribute, IRuleArgs, IValidator, IPropertyConstraint
	{
		public IsEvenAttribute()
		{
			this.Message = "Odd number";
		}

		public String Message
		{
			get;
			set;
		}

		public Boolean IsValid(Object value, IConstraintValidatorContext constraintValidatorContext)
		{
			Int32 number = Convert.ToInt32(value);

			return ((number % 2) == 0);
		}

		public void Apply(Property property)
		{
			Column column = property.ColumnIterator.OfType<Column>().First();
			column.CheckConstraint = String.Format("({0} % 2 = 0)", column.Name);
		}
	}
}
