using System.Diagnostics;

namespace RoboContainer.Core
{
	[DebuggerDisplay("{Contract}")]
	public class SimpleContractDeclaration<TContract> : ContractDeclaration
	{
		public TContract Contract { get; private set; }

		public SimpleContractDeclaration(TContract contract)
		{
			Contract = contract;
		}

		public override string ToString()
		{
			return Contract.ToString();
		}

		public bool Equals(SimpleContractDeclaration<TContract> other)
		{
			if(ReferenceEquals(null, other)) return false;
			if(ReferenceEquals(this, other)) return true;
			return Equals(other.Contract, Contract);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != typeof(SimpleContractDeclaration<TContract>)) return false;
			return Equals((SimpleContractDeclaration<TContract>) obj);
		}

		public override int GetHashCode()
		{
			return Contract.GetHashCode();
		}
	}
}