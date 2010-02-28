using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredPlugin : IDisposable
	{
		IEnumerable<ContractRequirement> RequiredContracts { get; }
		IEnumerable<IConfiguredPluggable> GetPluggables();
		IEnumerable<IConfiguredPluggable> GetExplicitlySpecifiedPluggables();
		bool UseAutoFoundPluggables { get; }
		IEnumerable<IConfiguredPluggable> GetAutoFoundPluggables();
	}
}