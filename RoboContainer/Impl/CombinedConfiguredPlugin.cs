using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class CombinedConfiguredPlugin : IConfiguredPlugin
	{
		private readonly IConfiguredPlugin parent;
		private readonly IConfiguredPlugin child;
		private readonly IContainerConfiguration configuration;
		private IConfiguredPluggable[] pluggables;

		public CombinedConfiguredPlugin(IConfiguredPlugin parent, IConfiguredPlugin child, IContainerConfiguration configuration)
		{
			this.parent = parent;
			this.child = child;
			this.configuration = configuration;
		}

		public IEnumerable<ContractRequirement> RequiredContracts
		{
			get { return parent.RequiredContracts.Union(child.RequiredContracts); }
		}

		public bool IsPluggableIgnored(Type pluggableType)
		{
			return child.IsPluggableIgnored(pluggableType) || parent.IsPluggableIgnored(pluggableType);
		}

		public InitializePluggableDelegate<object> InitializePluggable
		{
			get { return child.InitializePluggable ?? parent.InitializePluggable; }
		}

		public IEnumerable<IConfiguredPluggable> GetPluggables(IConstructionLogger constructionLogger)
		{
			return pluggables ?? (pluggables = this.CreatePluggables(constructionLogger));
		}

		public IEnumerable<IConfiguredPluggable> GetExplicitlySpecifiedPluggables(IConstructionLogger logger)
		{
			return parent.GetExplicitlySpecifiedPluggables(logger)
				.Select(p => new ConfiguredByPluginPluggable(this, child.InitializePluggable, p, configuration) as IConfiguredPluggable)
				.Concat(child.GetExplicitlySpecifiedPluggables(logger))
				.Select(p => new ConfiguredByPluginPluggable(this, parent.InitializePluggable, p, configuration) as IConfiguredPluggable);
		}

		public bool? AutoSearch
		{
			get { return child.AutoSearch ?? parent.AutoSearch; }
		}

		public bool ReuseSpecified
		{
			get { return parent.ReuseSpecified || child.ReuseSpecified; }
		}

		public IReusePolicy ReusePolicy
		{
			get { return child.ReuseSpecified ? child.ReusePolicy : parent.ReusePolicy; }
		}

		public IEnumerable<IConfiguredPluggable> GetAutoFoundPluggables(IConstructionLogger logger, bool filterByContractsRequirements)
		{
			return parent.GetAutoFoundPluggables(logger, false)
				.Select(p => configuration.GetConfiguredPluggable(p.PluggableType))
				.Select(p => new ConfiguredByPluginPluggable(this, child.InitializePluggable, p, configuration) as IConfiguredPluggable)
				.Exclude(p => p.Ignored || child.IsPluggableIgnored(p.PluggableType))
				.Where(p => !filterByContractsRequirements || p.ByContractsFilterWithLogging(RequiredContracts, logger));
		}

		public void Dispose()
		{
			if(pluggables != null) pluggables.ForEach(p => p.Dispose());
			pluggables = null;
		}
	}
}