using System.Diagnostics;

namespace RoboContainer.Core
{
	[DebuggerDisplay("{Contract}")]
	public class SimpleContractRequirement<TContract> : ContractRequirement
	{
		public SimpleContractRequirement(TContract contract)
		{
			Contract = contract;
		}

		public TContract Contract { get; private set; }

		public bool Equals(SimpleContractRequirement<TContract> other)
		{
			if(ReferenceEquals(null, other)) return false;
			if(ReferenceEquals(this, other)) return true;
			return Equals(other.Contract, Contract);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != typeof(SimpleContractRequirement<TContract>)) return false;
			return Equals((SimpleContractRequirement<TContract>) obj);
		}

		public override int GetHashCode()
		{
			return Contract.GetHashCode();
		}

		public override string ToString()
		{
			return Contract.ToString();
		}

		public override bool Satisfy(ContractDeclaration declaration)
		{
			var castedDeclaration = declaration as SimpleContractDeclaration<TContract>;
			return castedDeclaration != null && ContractEquals(Contract, castedDeclaration.Contract);
		}

		protected virtual bool ContractEquals(TContract required, TContract declared)
		{
			return required.Equals(declared);
		}
	}
}