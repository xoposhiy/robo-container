using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConfiguredByPluginPluggable : IConfiguredPluggable
	{
		private readonly IConfiguredPluggable configuredPluggable;
		private readonly PluginConfigurator pluginConfigurator;
		private IInstanceFactory factory;

		public ConfiguredByPluginPluggable(PluginConfigurator pluginConfigurator, IConfiguredPluggable configuredPluggable)
		{
			this.pluginConfigurator = pluginConfigurator;
			this.configuredPluggable = configuredPluggable;
			if (!(configuredPluggable.GetFactory() is ByConstructorInstanceFactory))
				throw new ArgumentException("factory should be ByConstructorInstanceFactory", "configuredPluggable");
		}

		public Type PluggableType
		{
			get { return configuredPluggable.PluggableType; }
		}

		public bool Ignored
		{
			get { return configuredPluggable.Ignored; }
		}

		public Func<IReuse> ReusePolicy
		{
			get { return pluginConfigurator.ReuseSpecified ? pluginConfigurator.ReusePolicy : configuredPluggable.ReusePolicy; }
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
			return new ConfiguredByPluginPluggable(pluginConfigurator, configuredPluggable.TryGetClosedGenericPluggable(closedGenericPluginType));
		}

		public IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts
		{
			get { return configuredPluggable.ExplicitlyDeclaredContracts; }
		}

		public IEnumerable<IConfiguredDependency> Dependencies
		{
			get { return configuredPluggable.Dependencies; }
		}

		private IInstanceFactory CreateFactory()
		{
			if(pluginConfigurator.ReuseSpecified && 
				pluginConfigurator.ReusePolicy != configuredPluggable.ReusePolicy || pluginConfigurator.InitializePluggable != null)
				return new ByConstructorInstanceFactory(this);
			return configuredPluggable.GetFactory();
		}
	}
}