namespace RoboContainer.Impl
{
	public class NamedRequirement : IContractRequirement
	{
		public NamedRequirement(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}
}