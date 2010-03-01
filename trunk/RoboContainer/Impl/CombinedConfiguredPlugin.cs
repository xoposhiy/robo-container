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

		public InitializePluggableDelegate<object> InitializePluggable
		{
			get { return child.InitializePluggable ?? parent.InitializePluggable; }
		}

		public IEnumerable<IConfiguredPluggable> GetPluggables()
		{
			return pluggables ?? (pluggables = this.CreatePluggables());
		}

		public IEnumerable<IConfiguredPluggable> GetExplicitlySpecifiedPluggables()
		{
			return parent.GetExplicitlySpecifiedPluggables()
				.Select(p => new ConfiguredByPluginPluggable(this, child.InitializePluggable, p, configuration) as IConfiguredPluggable)
				.Concat(child.GetExplicitlySpecifiedPluggables())
				.Select(p => new ConfiguredByPluginPluggable(this, parent.InitializePluggable, p, configuration) as IConfiguredPluggable);
		}

		public bool UseAutoFoundPluggables
		{
			get
			{
				bool parentHasExplicits = parent.GetExplicitlySpecifiedPluggables().Any();
				bool childHasExplicits = child.GetExplicitlySpecifiedPluggables().Any();
				return !parentHasExplicits && !childHasExplicits
					|| (parentHasExplicits && parent.UseAutoFoundPluggables)
						|| (childHasExplicits && child.UseAutoFoundPluggables);
			}
		}

		public bool ReuseSpecified
		{
			get { return parent.ReuseSpecified || child.ReuseSpecified; }
		}

		public IReusePolicy ReusePolicy
		{
			get { return child.ReuseSpecified ? child.ReusePolicy : parent.ReusePolicy; }
		}

		//TODO Учитывать DontUse, ReusePolicy и SetInitializer child конфигурации.
		public IEnumerable<IConfiguredPluggable> GetAutoFoundPluggables()
		{
			IConstructionLogger logger = configuration.GetConfiguredLogging().GetLogger();
			if(UseAutoFoundPluggables)
				return parent.GetAutoFoundPluggables()
					.Where(p => p.ByContractsFilter(RequiredContracts, logger))
					.Select(p => new ChildConfiguredPluggable(p, configuration) as IConfiguredPluggable);
			return Enumerable.Empty<IConfiguredPluggable>();
		}

		public void Dispose()
		{
			if(pluggables != null) pluggables.ForEach(p => p.Dispose());
			pluggables = null;
		}
	}
}