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
		IReusePolicy ReusePolicy { get; }
		bool ReuseSpecified { get; }
		InitializePluggableDelegate<object> InitializePluggable { get; }
		IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts { get; }
		IEnumerable<IConfiguredDependency> Dependencies { get; }
		Type[] InjectableConstructorArgsTypes { get; }
		IInstanceFactory GetFactory();
		IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType);
	}

	public static class ConfiguredPluginExtensions
	{
		public static IConfiguredPluggable[] CreatePluggables(this IConfiguredPlugin plugin)
		{
			var explicitlySpecifiedPluggables = plugin.GetExplicitlySpecifiedPluggables();
			if(plugin.AutoSearch.GetValueOrDefault(true))
				explicitlySpecifiedPluggables = explicitlySpecifiedPluggables.Concat(plugin.GetAutoFoundPluggables());
			return explicitlySpecifiedPluggables.ToArray();
		}

		public static IEnumerable<ContractDeclaration> GetAllContracts(this IConfiguredPluggable configuredPluggable)
		{
			IEnumerable<ContractDeclaration> cs = configuredPluggable.ExplicitlyDeclaredContracts;
			return !cs.Any() ? new[] {ContractDeclaration.Default} : cs;
		}

		public static bool ByContractsFilterWithLogging(this IConfiguredPluggable p,
			IEnumerable<ContractRequirement> requiredContracts, IConstructionLogger logger)
		{
			bool fitContracts = requiredContracts.All(req => p.GetAllContracts().Any(c => c.Satisfy(req)));
			if(!fitContracts)
			{
				logger.Declined(
					p.PluggableType,
					string.Format(
						"declared [{0}], required [{1}]",
						p.GetAllContracts().Select(c => c.ToString()).Join(", "),
						requiredContracts.Select(c => c.ToString()).Join(", "))
					);
			}
			return fitContracts;
		}
	}
}