using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredPlugin
	{
		IEnumerable<ContractRequirement> RequiredContracts { get; }
		IEnumerable<IConfiguredPluggable> GetPluggables();
	}
}