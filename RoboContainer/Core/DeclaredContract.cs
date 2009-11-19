using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public abstract class DeclaredContract
	{
		public abstract bool Satisfy(ContractRequirement requirement);

		public static implicit operator DeclaredContract(string contractName)
		{
			return new NamedContract(contractName);
		}
	}
}