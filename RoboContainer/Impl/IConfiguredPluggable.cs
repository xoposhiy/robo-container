using System;
using System.Collections.Generic;

namespace RoboContainer.Impl
{
	public interface IConfiguredPluggable
	{
		Type PluggableType { get; }
		bool Ignored { get; }
		InstanceLifetime Scope { get; }
		EnrichPluggableDelegate EnrichPluggable { get; }
		IInstanceFactory GetFactory();
		IEnumerable<IDeclaredContract> Contracts { get; }
	}
}