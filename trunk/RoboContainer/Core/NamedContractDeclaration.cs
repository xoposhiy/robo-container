using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public class NamedContractDeclaration : BaseContractDeclaration<NamedRequirement>
	{
		private readonly string contractName;

		public NamedContractDeclaration(string contractName)
		{
			this.contractName = contractName;
		}

		protected override bool Satisfy(NamedRequirement requirement)
		{
			return contractName == requirement.Name;
		}
	}
}