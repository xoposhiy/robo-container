using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredPluggable : IDisposable
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
			IEnumerable<ContractDeclaration> cs = configuredPluggable.ExplicitlyDeclaredContracts;
			return !cs.Any() ? new[] {ContractDeclaration.Default} : cs;
		}
	}
}