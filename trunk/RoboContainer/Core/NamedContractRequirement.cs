namespace RoboContainer.Core
{
	public class NamedContractRequirement : ContractRequirement
	{
		public NamedContractRequirement(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}
}