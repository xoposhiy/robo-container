using System.Collections.Generic;

namespace RoboContainer.Impl
{
	public interface IConfiguredDependency
	{
		IEnumerable<IContractRequirement> Contracts { get; }
	}
}