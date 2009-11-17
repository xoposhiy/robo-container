namespace RoboContainer.Impl
{
	public class NamedContract : BaseDeclaredContract<NamedRequirement>
	{
		private readonly string contractName;

		public NamedContract(string contractName)
		{
			this.contractName = contractName;
		}

		protected override bool Satisfy(NamedRequirement requirement)
		{
			return contractName == requirement.Name;
		}
	}
}