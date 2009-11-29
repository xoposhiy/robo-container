using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public class NamedContractDeclaration : BaseContractDeclaration<NamedContractRequirement>
	{
		private readonly string contractName;

		public NamedContractDeclaration(string contractName)
		{
			this.contractName = contractName;
		}

		public override string ToString()
		{
			return contractName;
		}

		protected override bool Satisfy(NamedContractRequirement requirement)
		{
			return contractName == requirement.Name;
		}
	}
}