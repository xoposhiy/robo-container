using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class CombinedConfiguredPlugin : IConfiguredPlugin
	{
		private readonly IConfiguredPlugin parent;
		private readonly IConfiguredPlugin child;
		private readonly ChildConfiguration childConfiguration;
		private IConfiguredPluggable[] pluggables;

		public CombinedConfiguredPlugin(IConfiguredPlugin parent, IConfiguredPlugin child, ChildConfiguration childConfiguration)
		{
			this.parent = parent;
			this.child = child;
			this.childConfiguration = childConfiguration;
		}

		public Type PluginType
		{
			get
			{
				Debug.Assert(child.PluginType == parent.PluginType);
				return parent.PluginType;
			}
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
			return pluggables ?? (pluggables = this.CreatePluggables(constructionLogger, childConfiguration));
		}

		public IEnumerable<IConfiguredPluggable> GetExplicitlySpecifiedPluggables(IConstructionLogger logger)
		{
			return 
				parent.GetExplicitlySpecifiedPluggables(logger)
				.Select(p => new CombinedConfiguredPluggable(p, childConfiguration.GetChildConfiguredPluggable(p), childConfiguration))
				.Cast<IConfiguredPluggable>()
				.Concat(child.GetExplicitlySpecifiedPluggables(logger));
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

		public IEnumerable<IConfiguredPluggable> GetAutoFoundPluggables(IConstructionLogger logger)
		{
			return this.GetAutoFoundPluggables(childConfiguration, logger);
		}

		public void Dispose()
		{
			if(pluggables != null) pluggables.ForEach(p => p.Dispose());
			pluggables = null;
		}
	}
}