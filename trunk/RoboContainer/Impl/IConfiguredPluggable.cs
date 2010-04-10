using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredPluggable : IDisposable
	{
		[CanBeNull]
		Type PluggableType { get; }
		bool Ignored { get; }
		IReusePolicy ReusePolicy { get; }
		bool ReuseSpecified { get; }
		[CanBeNull]
		InitializePluggableDelegate<object> InitializePluggable { get; }
		IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts { get; }
		DependenciesBag Dependencies { get; }
		[CanBeNull]
		Type[] InjectableConstructorArgsTypes { get; }
		IInstanceFactory GetFactory();
		[CanBeNull]
		IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType);
		void DumpDebugInfo(Action<string> writeLine);
	}

	public static class ConfiguredPluggableExtensions
	{
		public static IEnumerable<ContractDeclaration> AllDeclaredContracts(this IConfiguredPluggable pluggable)
		{
			if(pluggable.ExplicitlyDeclaredContracts.Any()) return pluggable.ExplicitlyDeclaredContracts;
			return new []{ContractDeclaration.Default};
		}

		public static string DumpDebugInfo(this IConfiguredPluggable pluggable)
		{
			var b = new StringBuilder();
			pluggable.DumpDebugInfo(s => b.AppendLine(s));
			return b.ToString();
		}

		public static void DumpMainInfo(this IConfiguredPluggable pluggable, Action<string> writeLine)
		{
			writeLine(pluggable.GetType().Name);
			if (pluggable.PluggableType != null) writeLine("\ttype: " + pluggable.PluggableType.Name);
			if (pluggable.Ignored) writeLine("\tignored");
			if (pluggable.InitializePluggable != null) writeLine("\twith initializer: " + pluggable.InitializePluggable.Method);
			if (pluggable.ReuseSpecified) writeLine("\treuse policy: " + pluggable.ReusePolicy.GetType().Name);
			if (pluggable.ExplicitlyDeclaredContracts.Any())
				writeLine("\tdeclared contracts: " + string.Join(", ", pluggable.ExplicitlyDeclaredContracts.Select(c => c.ToString()).ToArray()));
		}
	}
	public static class ConfiguredPluginExtensions
	{
		public static IConfiguredPluggable ApplyPluginConfiguration(this IConfiguredPlugin plugin, IConfiguredPluggable configuredPluggable, IContainerConfiguration configuration)
		{
			if(configuredPluggable.PluggableType == null) return configuredPluggable;
			return new ConfiguredByPluginPluggable(plugin, configuredPluggable, configuration);
		}

		public static IEnumerable<IConfiguredPluggable> GetAutoFoundPluggables(this IConfiguredPlugin plugin, IContainerConfiguration configuration, IConstructionLogger logger)
		{
			return
				configuration.GetScannableTypes(plugin.PluginType)
					.Select(t => configuration.TryGetConfiguredPluggable(plugin.PluginType, t))
					.Where(pluggable => pluggable != null);
		}

		public static IConfiguredPluggable[] CreatePluggables(this IConfiguredPlugin plugin, IConstructionLogger logger, IContainerConfiguration configuration)
		{
			var explicitlySpecifiedPluggables =
				plugin.GetExplicitlySpecifiedPluggables(logger)
					.Select(p => plugin.ApplyPluginConfiguration(p, configuration));
			if(plugin.AutoSearch.GetValueOrDefault(true))
			{
				var autoFoundPluggables =
					plugin.GetAutoFoundPluggables(logger)
					.Select(p => plugin.ApplyPluginConfiguration(p, configuration))
					.Exclude(p => p.Ignored)
					.Where(pluggable => pluggable.ByContractsFilterWithLogging(plugin.RequiredContracts, logger));
				explicitlySpecifiedPluggables = explicitlySpecifiedPluggables.Concat(autoFoundPluggables);
			}
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
			bool fitContracts = requiredContracts.All(req => p.GetAllContracts().Any(req.Satisfy));
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