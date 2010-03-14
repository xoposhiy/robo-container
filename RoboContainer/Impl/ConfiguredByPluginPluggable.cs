using System;
using System.Collections.Generic;
using System.Diagnostics;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	[DebuggerDisplay("ByPlugin {PluggableType}")]
	public class ConfiguredByPluginPluggable : IConfiguredPluggable
	{
		private readonly IConfiguredPluggable configuredPluggable;
		private readonly IContainerConfiguration configuration;
		private readonly IConfiguredPlugin configuredPlugin;
		private IInstanceFactory factory;

		public ConfiguredByPluginPluggable(IConfiguredPlugin configuredPlugin, IConfiguredPluggable configuredPluggable, IContainerConfiguration configuration)
		{
			this.configuredPlugin = configuredPlugin;
			this.configuredPluggable = configuredPluggable;
			this.configuration = configuration;
		}

		public Type PluggableType
		{
			get { return configuredPluggable.PluggableType; }
		}

		public bool Ignored
		{
			get { return configuredPluggable.Ignored || configuredPlugin.IsPluggableIgnored(PluggableType); }
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
			get { return configuredPluggable.InitializePluggable.CombineWith(configuredPlugin.InitializePluggable); }
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

		public void DumpDebugInfo(Action<string> writeLine)
		{
			this.DumpMainInfo(writeLine);
			writeLine("\tPlugin " + configuredPlugin.PluginType.Name);
			configuredPluggable.DumpDebugInfo(l => writeLine("\t" + l));
		}

		public IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts
		{
			get { return configuredPluggable.ExplicitlyDeclaredContracts; }
		}

		public DependenciesBag Dependencies
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
			if(configuredPlugin.ReuseSpecified || configuredPlugin.InitializePluggable != null)
				return configuredPluggable.GetFactory().CreateByPrototype(this, configuredPlugin.ReusePolicy, InitializePluggable, configuration);
			return configuredPluggable.GetFactory();
		}
	}
}