namespace RoboContainer.Core
{
	public abstract class ContractRequirement
	{
		static ContractRequirement()
		{
			Default = new DefaultContractRequirement();
		}

		public static ContractRequirement Default { get; private set; }

		public static implicit operator ContractRequirement(string value)
		{
			return new NamedContractRequirement(value);
		}

		private class DefaultContractRequirement : ContractRequirement
		{
			public override string ToString()
			{
				return "DEFAULT";
			}
		}
	}
}