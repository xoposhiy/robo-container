using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public abstract class BaseDeclaredContract<TContractRequirement> : DeclaredContract where TContractRequirement : ContractRequirement
	{
		public override bool Satisfy(ContractRequirement requirement)
		{
			if (typeof (TContractRequirement) != requirement.GetType()) return false;
			return Satisfy((TContractRequirement) requirement);
		}

		protected abstract bool Satisfy(TContractRequirement requirement);
	}
}