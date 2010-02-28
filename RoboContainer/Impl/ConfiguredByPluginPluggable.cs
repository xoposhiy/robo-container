using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConfiguredByPluginPluggable : IConfiguredPluggable
	{
		private readonly IConfiguredPluggable configuredPluggable;
		private readonly IContainerConfiguration configuration;
		private readonly PluginConfigurator pluginConfigurator;
		private IInstanceFactory factory;

		public ConfiguredByPluginPluggable(PluginConfigurator pluginConfigurator, IConfiguredPluggable configuredPluggable, IContainerConfiguration configuration)
		{
			this.pluginConfigurator = pluginConfigurator;
			this.configuredPluggable = configuredPluggable;
			this.configuration = configuration;
		}

		public Type PluggableType
		{
			get { return configuredPluggable.PluggableType; }
		}

		public bool Ignored
		{
			get { return configuredPluggable.Ignored; }
		}

		public IReusePolicy ReusePolicy
		{
			get { return pluginConfigurator.ReuseSpecified ? pluginConfigurator.ReusePolicy : configuredPluggable.ReusePolicy; }
		}

		public bool ReuseSpecified
		{
			get { return pluginConfigurator.ReuseSpecified || configuredPluggable.ReuseSpecified; }
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

		public IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType)
		{
			return new ConfiguredByPluginPluggable(pluginConfigurator, configuredPluggable.TryGetClosedGenericPluggable(closedGenericPluginType), configuration);
		}

		public IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts
		{
			get { return configuredPluggable.ExplicitlyDeclaredContracts; }
		}

		public IEnumerable<IConfiguredDependency> Dependencies
		{
			get { return configuredPluggable.Dependencies; }
		}

		public void Dispose()
		{
			if(factory == configuredPluggable.GetFactory()) factory = null; // Не трогаем чужие фабрики.
			else DisposeUtils.Dispose(ref factory);
		}

		private IInstanceFactory CreateFactory()
		{
			if(pluginConfigurator.ReuseSpecified || pluginConfigurator.InitializePluggable != null)
				return configuredPluggable.GetFactory().CreateByPrototype(pluginConfigurator.ReusePolicy, pluginConfigurator.InitializePluggable, configuration);
			return configuredPluggable.GetFactory();
		}
	}
}