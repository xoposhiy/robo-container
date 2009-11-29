using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredPlugin
	{
		IEnumerable<IConfiguredPluggable> GetPluggables();
		IEnumerable<ContractRequirement> RequiredContracts { get; }
	}
}