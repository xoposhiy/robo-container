using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConfiguredByDelegatePluggable : IConfiguredPluggable
	{
		private readonly CreatePluggableDelegate<object> createPluggable;
		private readonly PluginConfigurator pluginConfigurator;
		private ByDelegateInstanceFactory factory;

		public ConfiguredByDelegatePluggable(PluginConfigurator pluginConfigurator, CreatePluggableDelegate<object> createPluggable)
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

		public Func<IReuse> ReusePolicy
		{
			get { return pluginConfigurator.ReusePolicy; }
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
			return factory ?? (factory = new ByDelegateInstanceFactory(ReusePolicy, InitializePluggable, createPluggable));
		}

		public IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType)
		{
			return new ConfiguredByDelegatePluggable(pluginConfigurator, createPluggable); 
		}

		public IEnumerable<ContractDeclaration> Contracts
		{
			get { yield break; }
		}

		public IEnumerable<IConfiguredDependency> Dependencies
		{
			get { throw new NotSupportedException(); }
		}
	}
}