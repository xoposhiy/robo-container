using System.Collections.Generic;

namespace RoboContainer.Impl
{
	public interface IConfiguredDependency
	{
		IEnumerable<ContractRequirement> Contracts { get; }
	}
}