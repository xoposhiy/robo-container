using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredPluggable
	{
		Type PluggableType { get; }
		bool Ignored { get; }
		Func<IReuse> ReusePolicy { get; }
		InitializePluggableDelegate<object> InitializePluggable { get; }
		IEnumerable<ContractDeclaration> Contracts { get; }
		IEnumerable<IConfiguredDependency> Dependencies { get; }
		Type[] InjectableConstructorArgsTypes { get; }
		IInstanceFactory GetFactory();
		IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType);
	}
}