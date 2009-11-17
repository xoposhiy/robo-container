using System;
using System.Collections.Generic;

namespace RoboContainer.Impl
{
	internal class ByDelegatePluggable : IConfiguredPluggable
	{
		private readonly CreatePluggableDelegate createPluggable;
		private readonly PluginConfigurator pluginConfigurator;

		public ByDelegatePluggable(PluginConfigurator pluginConfigurator, CreatePluggableDelegate createPluggable)
		{
			this.pluginConfigurator = pluginConfigurator;
			this.createPluggable = createPluggable;
		}

		public Type PluggableType
		{
			get { return null; }
		}

		public bool Ignored
		{
			get { return false; }
		}

		public InstanceLifetime Scope
		{
			get { return pluginConfigurator.Scope; }
		}

		public EnrichPluggableDelegate EnrichPluggable
		{
			get { return pluginConfigurator.EnrichPluggable; }
		}

		public IInstanceFactory GetFactory()
		{
			return new DelegateInstanceFactory(Scope, EnrichPluggable, createPluggable);
		}

		public IEnumerable<IDeclaredContract> Contracts
		{
			get { yield break; }
		}
	}
}