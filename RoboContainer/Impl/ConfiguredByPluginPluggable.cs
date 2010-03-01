using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConfiguredByPluginPluggable : IConfiguredPluggable
	{
		private readonly IConfiguredPluggable configuredPluggable;
		private readonly IContainerConfiguration configuration;
		private readonly IConfiguredPlugin configuredPlugin;
		private readonly InitializePluggableDelegate<object> initializePluggable;
		private IInstanceFactory factory;

		public ConfiguredByPluginPluggable(IConfiguredPlugin configuredPlugin, IConfiguredPluggable configuredPluggable, IContainerConfiguration configuration)
		{
			this.configuredPlugin = configuredPlugin;
			this.configuredPluggable = configuredPluggable;
			this.configuration = configuration;
		}

		public ConfiguredByPluginPluggable(CombinedConfiguredPlugin configuredPlugin, InitializePluggableDelegate<object> initializePluggable, IConfiguredPluggable configuredPluggable, IContainerConfiguration configuration)
		{
			this.configuredPlugin = configuredPlugin;
			this.initializePluggable = initializePluggable;
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
			get { return configuredPlugin.ReuseSpecified ? configuredPlugin.ReusePolicy : configuredPluggable.ReusePolicy; }
		}

		public bool ReuseSpecified
		{
			get { return configuredPlugin.ReuseSpecified || configuredPluggable.ReuseSpecified; }
		}

		public InitializePluggableDelegate<object> InitializePluggable
		{
			get
			{
				var pluginInitializator = initializePluggable ?? configuredPlugin.InitializePluggable;
				return
					pluginInitializator == null
						?
							configuredPluggable.InitializePluggable
						:
							configuredPluggable.InitializePluggable == null
								?
									pluginInitializator
								:
									(o, container) =>
										pluginInitializator(configuredPluggable.InitializePluggable(o, container), container);
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
			return new ConfiguredByPluginPluggable(configuredPlugin, configuredPluggable.TryGetClosedGenericPluggable(closedGenericPluginType), configuration);
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
			var pluginInitializator = initializePluggable ?? configuredPlugin.InitializePluggable;
			if(configuredPlugin.ReuseSpecified || pluginInitializator != null)
				return configuredPluggable.GetFactory().CreateByPrototype(configuredPlugin.ReusePolicy, pluginInitializator, configuration);
			return configuredPluggable.GetFactory();
		}
	}
}