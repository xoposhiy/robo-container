namespace RoboContainer.Impl
{
	public abstract class BaseDeclaredContract<TContractRequirement> : IDeclaredContract where TContractRequirement : IContractRequirement
	{
		public bool Satisfy(IContractRequirement requirement)
		{
			if(typeof(TContractRequirement) != requirement.GetType()) return false;
			return Satisfy((TContractRequirement) requirement);
		}

		protected abstract bool Satisfy(TContractRequirement requirement);
	}
}