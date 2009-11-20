using System;

namespace RoboContainer.Core
{
	public interface IDependencyConfigurator
	{
		IDependencyConfigurator RequireContract(params string[] requiredContracts);
		IDependencyConfigurator UseValue(object o);
		IDependencyConfigurator UsePluggable(Type pluggableType);
		IDependencyConfigurator UsePluggable<TPluggable>();
	}
}