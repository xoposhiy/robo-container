namespace RoboContainer.Impl
{
	public interface IDeclaredContract
	{
		bool Satisfy(IContractRequirement requirement);
	}
}