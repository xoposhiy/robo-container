using System;
using System.Collections.Generic;
using RoboContainer.Core;
using System.Linq;

namespace RoboContainer.Impl
{
	public interface IConfiguredPluggable
	{
		Type PluggableType { get; }
		bool Ignored { get; }
		Func<IReuse> ReusePolicy { get; }
		InitializePluggableDelegate<object> InitializePluggable { get; }
		IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts { get; }
		IEnumerable<IConfiguredDependency> Dependencies { get; }
		Type[] InjectableConstructorArgsTypes { get; }
		IInstanceFactory GetFactory();
		IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType);
	}

	public static class ConfiguredPluginExtensions
	{
		public static IEnumerable<ContractDeclaration> GetAllContracts(this IConfiguredPluggable configuredPluggable)
		{
			var cs = configuredPluggable.ExplicitlyDeclaredContracts;
			return !cs.Any() ? new[] {ContractDeclaration.Default} : cs;
		}
	}
}