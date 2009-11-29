namespace RoboContainer.Core
{
	public class NamedContractRequirement : ContractRequirement
	{
		public NamedContractRequirement(string name)
		{
			Name = name;
		}

		public override string ToString()
		{
			return Name;
		}

		public string Name { get; private set; }
	}
}