using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConfiguredByDelegatePluggable : IConfiguredPluggable
	{
		private readonly ContractDeclaration[] declaredContracts;
		private readonly IContainerConfiguration configuration;
		private readonly CreatePluggableDelegate<object> createPluggable;
		private readonly PluginConfigurator pluginConfigurator;
		private ByDelegateInstanceFactory factory;

		public ConfiguredByDelegatePluggable(
			PluginConfigurator pluginConfigurator, 
			CreatePluggableDelegate<object> createPluggable, 
			ContractDeclaration[] declaredContracts, 
			IContainerConfiguration configuration)
		{
			this.pluginConfigurator = pluginConfigurator;
			this.createPluggable = createPluggable;
			this.declaredContracts = declaredContracts;
			this.configuration = configuration;
		}

		public Type PluggableType
		{
			get { return null; }
		}

		public bool Ignored
		{
			get { return false; }
		}

		public IReusePolicy ReusePolicy
		{
			get { return pluginConfigurator.ReusePolicy; }
		}

		public bool ReuseSpecified
		{
			get { return pluginConfigurator.ReuseSpecified; }
		}

		public InitializePluggableDelegate<object> InitializePluggable
		{
			get { return pluginConfigurator.InitializePluggable; }
		}

		public Type[] InjectableConstructorArgsTypes
		{
			get { return null; }
		}

		public IInstanceFactory GetFactory()
		{
			return factory ?? (factory = new ByDelegateInstanceFactory(ReusePolicy, InitializePluggable, createPluggable, configuration));
		}

		public IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType)
		{
			return new ConfiguredByDelegatePluggable(pluginConfigurator, createPluggable, declaredContracts, configuration);
		}

		public void DumpDebugInfo(Action<string> writeLine)
		{
			this.DumpMainInfo(writeLine);
		}

		public IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts
		{
			get { return declaredContracts; }
		}

		public DependenciesBag Dependencies
		{
			get { return null; }
		}

		public void Dispose()
		{
			DisposeUtils.Dispose(ref factory);
		}
	}
}