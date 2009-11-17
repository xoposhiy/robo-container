using System;
using System.Collections.Generic;

namespace RoboContainer.Impl
{
	internal class ByPluginConfiguredPluggable : IConfiguredPluggable
	{
		private readonly IConfiguredPluggable configuredPluggable;
		private readonly PluginConfigurator pluginConfigurator;
		private IInstanceFactory factory;

		public ByPluginConfiguredPluggable(PluginConfigurator pluginConfigurator, IConfiguredPluggable configuredPluggable)
		{
			this.pluginConfigurator = pluginConfigurator;
			this.configuredPluggable = configuredPluggable;
		}

		public Type PluggableType
		{
			get { return configuredPluggable.PluggableType; }
		}

		public bool Ignored
		{
			get { return configuredPluggable.Ignored; }
		}

		public InstanceLifetime Scope
		{
			get { return pluginConfigurator.ScopeSpecified ? pluginConfigurator.Scope : configuredPluggable.Scope; }
		}

		public EnrichPluggableDelegate EnrichPluggable
		{
			get
			{
				return
					pluginConfigurator.EnrichPluggable == null
						?
							configuredPluggable.EnrichPluggable
						:
							configuredPluggable.EnrichPluggable == null
								?
									pluginConfigurator.EnrichPluggable
								:
									(o, container) =>
									pluginConfigurator.EnrichPluggable(configuredPluggable.EnrichPluggable(o, container), container);
			}
		}

		public IInstanceFactory GetFactory()
		{
			return factory ?? (factory = CreateFactory());
		}

		public IEnumerable<IDeclaredContract> Contracts
		{
			get { return configuredPluggable.Contracts; }
		}

		private IInstanceFactory CreateFactory()
		{
			if (pluginConfigurator.ScopeSpecified && pluginConfigurator.Scope != configuredPluggable.Scope || pluginConfigurator.EnrichPluggable != null)
				return new InstanceFactory(this);
			return configuredPluggable.GetFactory();
		}
	}
}