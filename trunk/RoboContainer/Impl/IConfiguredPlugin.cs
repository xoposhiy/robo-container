using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredPlugin : IDisposable
	{
		IEnumerable<ContractRequirement> RequiredContracts { get; }
		bool UseAutoFoundPluggables { get; }
		bool ReuseSpecified { get; }
		IReusePolicy ReusePolicy { get; }
		InitializePluggableDelegate<object> InitializePluggable { get; }
		IEnumerable<IConfiguredPluggable> GetPluggables();
		IEnumerable<IConfiguredPluggable> GetExplicitlySpecifiedPluggables();
		IEnumerable<IConfiguredPluggable> GetAutoFoundPluggables();
	}
}