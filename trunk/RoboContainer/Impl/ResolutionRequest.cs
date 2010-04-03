using System;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	internal class ResolutionRequest
	{
		public ResolutionRequest(Type requestedType, ContractRequirement[] requestedContracts)
		{
			RequestedType = requestedType;
			RequestedContracts = requestedContracts;
		}

		public bool Equals(ResolutionRequest other)
		{
			if(ReferenceEquals(null, other)) return false;
			if(ReferenceEquals(this, other)) return true;
			return Equals(other.RequestedType, RequestedType) && other.RequestedContracts.SequenceEqual(RequestedContracts);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != typeof(ResolutionRequest)) return false;
			return Equals((ResolutionRequest) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return RequestedType.GetHashCode()*397 ^ (RequestedContracts.Sum(c => c.GetHashCode()));
			}
		}

		public Type RequestedType { get; private set; }
		public ContractRequirement[] RequestedContracts { get; private set; }
	}
}