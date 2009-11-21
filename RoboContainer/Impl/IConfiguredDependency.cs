using System.Collections.Generic;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredDependency
	{
		IEnumerable<ContractRequirement> Contracts { get; }
		bool TryGetValue(ParameterInfo parameter, Container container, out object actualArg);
	}
}