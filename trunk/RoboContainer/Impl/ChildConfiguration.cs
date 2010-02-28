using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ChildConfiguration : ContainerConfiguration
	{
		private readonly IContainerConfiguration parent;
		private readonly IDictionary<Type, IConfiguredPluggable> pluggables = new Dictionary<Type, IConfiguredPluggable>();
		private readonly IDictionary<Type, IConfiguredPlugin> plugins = new Dictionary<Type, IConfiguredPlugin>();

		public ChildConfiguration(IContainerConfiguration parent)
		{
			this.parent = parent;
		}

		public override void Dispose()
		{
			base.Dispose();
			pluggables.Values.ForEach(c => c.Dispose());
			pluggables.Clear();
			plugins.Values.ForEach(c => c.Dispose());
			plugins.Clear();
		}

		public override bool HasAssemblies()
		{
			return base.HasAssemblies() || parent.HasAssemblies();
		}

		public override IEnumerable<Type> GetScannableTypes()
		{
			return base.GetScannableTypes().Union(parent.GetScannableTypes());
		}

		public override IEnumerable<Type> GetScannableTypes(Type pluginType)
		{
			return base.GetScannableTypes(pluginType).Union(parent.GetScannableTypes(pluginType));
		}

		public override IConfiguredPluggable GetConfiguredPluggable(Type pluggableType)
		{
			IConfiguredPluggable result;
			if(!pluggables.TryGetValue(pluggableType, out result))
			{
				result = new CombinedConfiguredPluggable(
					parent.GetConfiguredPluggable(pluggableType),
					base.GetConfiguredPluggable(pluggableType),
					this);
				pluggables.Add(pluggableType, result);
			}
			return result;
		}

		public override IConfiguredPlugin GetConfiguredPlugin(Type pluginType)
		{
			IConfiguredPlugin result;
			if(!plugins.TryGetValue(pluginType, out result))
			{
				result = new CombinedConfiguredPlugin(
					parent.GetConfiguredPlugin(pluginType),
					base.GetConfiguredPlugin(pluginType),
					this);
				plugins.Add(pluginType, result);
			}
			return result;
		}
	}

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

		public IEnumerable<IConfiguredPluggable> GetPluggables()
		{
			return pluggables ?? (pluggables = this.CreatePluggables());
		}

		public IEnumerable<IConfiguredPluggable> GetExplicitlySpecifiedPluggables()
		{
			return parent.GetExplicitlySpecifiedPluggables()
				.Select(p => new ChildConfiguredPluggable(p, configuration) as IConfiguredPluggable)
				.Concat(child.GetExplicitlySpecifiedPluggables());
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