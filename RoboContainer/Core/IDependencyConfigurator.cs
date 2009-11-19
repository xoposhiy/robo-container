using System;

namespace RoboContainer.Core
{
	public interface IDependencyConfigurator
	{
		void RequireContract(params string[] requiredContracts);
		void UseValue(object o);
		void UsePluggable(Type pluggableType);
		void UsePluggable<TPluggable>();
	}
}