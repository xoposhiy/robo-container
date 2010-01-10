using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredDependency
	{
		IEnumerable<ContractRequirement> Contracts { get; }
		bool TryGetValue(Type dependencyType, Container container, out object result);
		string Name { get; }
	}
}