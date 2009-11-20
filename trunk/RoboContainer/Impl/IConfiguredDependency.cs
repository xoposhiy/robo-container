using System.Collections.Generic;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredDependency
	{
		IEnumerable<ContractRequirement> Contracts { get; }
		object GetValue(ParameterInfo parameter, Container container);
	}
}