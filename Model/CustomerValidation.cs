using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Validator.Cfg.Loquacious;
using Succintly.Model;

namespace Succintly.Model
{
	public class CustomerValidation : ValidationDef<Customer>
	{
		public CustomerValidation()
		{
			this.ValidateInstance.By((customer, context) => customer.Address != null).WithMessage("The customer address is mandatory");
			this.Define(x => x.Name).NotNullableAndNotEmpty().WithMessage("The customer name is mandatory");
			this.Define(x => x.Name).MaxLength(50).WithMessage("The customer name can only have 50 characters");
			this.Define(x => x.Email).NotNullableAndNotEmpty().WithMessage("The customer email is mandatory");
			this.Define(x => x.Email).MaxLength(50).WithMessage("The customer email can only have 50 characters");
			this.Define(x => x.Email).IsEmail().WithMessage("The customer email must be a valid email adddress");
		}
	}
}
