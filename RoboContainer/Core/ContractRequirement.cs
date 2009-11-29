namespace RoboContainer.Core
{
	public abstract class ContractRequirement
	{
		private class DefaultContractRequirement : ContractRequirement
		{
			public override string ToString()
			{
				return "DEFAULT";
			}
		}

		static ContractRequirement()
		{
			Default = new DefaultContractRequirement();
		}

		public static implicit operator ContractRequirement(string value)
		{
			return new NamedContractRequirement(value);
		}

		public static ContractRequirement Default { get; private set; }
	}
}