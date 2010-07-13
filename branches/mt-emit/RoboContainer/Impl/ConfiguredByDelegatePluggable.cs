using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConfiguredByDelegatePluggable : IConfiguredPluggable
	{
		private readonly IContainerConfiguration configuration;
		private readonly CreatePluggableDelegate<object> createPluggable;
		private readonly string[] declaredContracts;
		private readonly Deferred<ByDelegateInstanceFactory> factory;
		private readonly PluginConfigurator pluginConfigurator;

		public ConfiguredByDelegatePluggable(
			PluginConfigurator pluginConfigurator,
			CreatePluggableDelegate<object> createPluggable,
			string[] declaredContracts,
			IContainerConfiguration configuration)
		{
			this.pluginConfigurator = pluginConfigurator;
			this.createPluggable = createPluggable;
			this.declaredContracts = declaredContracts;
			this.configuration = configuration;
			factory = new Deferred<ByDelegateInstanceFactory>(InstanceFactoryCreator, InstanceFactoryFinalizer);
		}

		#region IConfiguredPluggable Members

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
			return factory.Get();
		}

		public IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType)
		{
			return new ConfiguredByDelegatePluggable(pluginConfigurator, createPluggable, declaredContracts, configuration);
		}

		public void DumpDebugInfo(Action<string> writeLine)
		{
			this.DumpMainInfo(writeLine);
		}

		public IEnumerable<string> ExplicitlyDeclaredContracts
		{
			get { return declaredContracts; }
		}

		public DependenciesBag Dependencies
		{
			get { return null; }
		}

		public void Dispose()
		{
			factory.Dispose();
		}

		#endregion

		private ByDelegateInstanceFactory InstanceFactoryCreator()
		{
			return new ByDelegateInstanceFactory(ReusePolicy, InitializePluggable, createPluggable, configuration);
		}

		private static void InstanceFactoryFinalizer(ByDelegateInstanceFactory instanceFactory)
		{
			DisposeUtils.Dispose(ref instanceFactory);
		}
	}
}