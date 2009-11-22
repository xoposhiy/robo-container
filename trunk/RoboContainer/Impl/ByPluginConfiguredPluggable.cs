using System;
using System.Collections.Generic;
using RoboContainer.Core;

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

		public Func<ILifetime> Scope
		{
			get { return pluginConfigurator.ScopeSpecified ? pluginConfigurator.Lifetime : configuredPluggable.Scope; }
		}

		public InitializePluggableDelegate<object> InitializePluggable
		{
			get
			{
				return
					pluginConfigurator.InitializePluggable == null
						?
							configuredPluggable.InitializePluggable
						:
							configuredPluggable.InitializePluggable == null
								?
									pluginConfigurator.InitializePluggable
								:
									(o, container) =>
									pluginConfigurator.InitializePluggable(configuredPluggable.InitializePluggable(o, container), container);
			}
		}

		public Type[] InjectableConstructorArgsTypes
		{
			get { return configuredPluggable.InjectableConstructorArgsTypes; }
		}

		public IInstanceFactory GetFactory()
		{
			return factory ?? (factory = CreateFactory());
		}

		public IEnumerable<ContractDeclaration> Contracts
		{
			get { return configuredPluggable.Contracts; }
		}

		public IEnumerable<IConfiguredDependency> Dependencies
		{
			get { return configuredPluggable.Dependencies; }
		}

		private IInstanceFactory CreateFactory()
		{
			if(pluginConfigurator.ScopeSpecified && pluginConfigurator.Lifetime != configuredPluggable.Scope || pluginConfigurator.InitializePluggable != null)
				return new InstanceFactory(this);
			return configuredPluggable.GetFactory();
		}
	}
}