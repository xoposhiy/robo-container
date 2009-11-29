using System;

namespace RoboContainer.Core
{
	public abstract class ContractDeclaration
	{
		static ContractDeclaration()
		{
			Default = new DefaultContractDeclaration();
		}

		private class DefaultContractDeclaration : ContractDeclaration
		{
			public override bool Satisfy(ContractRequirement requirement)
			{
				return requirement == ContractRequirement.Default;
			}
			
			public override string ToString()
			{
				return "DEFAULT";
			}

		}

		public abstract bool Satisfy(ContractRequirement requirement);

		public static implicit operator ContractDeclaration(string contractName)
		{
			return new NamedContractDeclaration(contractName);
		}

		public static ContractDeclaration Default { get; private set; }
	}
}