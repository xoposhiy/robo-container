using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public abstract class ContractDeclaration
	{
		public abstract bool Satisfy(ContractRequirement requirement);

		public static implicit operator ContractDeclaration(string contractName)
		{
			return new NamedContractDeclaration(contractName);
		}
	}
}