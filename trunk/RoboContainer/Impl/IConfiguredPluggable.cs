using System;
using System.Collections.Generic;

namespace RoboContainer.Impl
{
	public interface IConfiguredPluggable
	{
		Type PluggableType { get; }
		bool Ignored { get; }
		InstanceLifetime Scope { get; }
		InitializePluggableDelegate InitializePluggable { get; }
		IInstanceFactory GetFactory();
		IEnumerable<DeclaredContract> Contracts { get; }
		IEnumerable<IConfiguredDependency> Dependencies { get; }
	}
}