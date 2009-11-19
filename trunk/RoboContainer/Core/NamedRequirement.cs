namespace RoboContainer.Core
{
	public class NamedRequirement : ContractRequirement
	{
		public NamedRequirement(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}
}