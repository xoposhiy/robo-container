using System;

namespace RoboContainer.Core
{
	public class StringContractRequirement : SimpleContractRequirement<string>
	{
		public StringContractRequirement(string contract)
			: base(contract)
		{
		}

		protected override bool ContractEquals(string required, string declared)
		{
			return required.Equals(declared, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}