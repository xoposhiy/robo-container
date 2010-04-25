using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	[DebuggerDisplay("ByType {PluggableType}")]
	public class ConfiguredTypePluggable : IConfiguredPluggable
	{
		private readonly string[] additionalDeclaredContracts;
		private readonly Func<IConfiguredPluggable> configuredPluggableProvider;
		private IConfiguredPluggable configuredPluggable;
		private IEnumerable<string> allDeclaredContracts;

		public ConfiguredTypePluggable(Func<IConfiguredPluggable> configuredPluggableProvider, string[] additionalDeclaredContracts)
		{
			this.configuredPluggableProvider = configuredPluggableProvider;
			this.additionalDeclaredContracts = additionalDeclaredContracts;
		}

		private IConfiguredPluggable ConfiguredPluggable
		{
			get { return configuredPluggable ?? (configuredPluggable = configuredPluggableProvider()); }
		}

		public Type PluggableType
		{
			get { return ConfiguredPluggable.PluggableType; }
		}

		public bool Ignored
		{
			get { return false; }
		}

		public IReusePolicy ReusePolicy
		{
			get { return ConfiguredPluggable.ReusePolicy; }
		}

		public bool ReuseSpecified
		{
			get { return ConfiguredPluggable.ReuseSpecified; }
		}

		public InitializePluggableDelegate<object> InitializePluggable
		{
			get { return ConfiguredPluggable.InitializePluggable; }
		}

		public IEnumerable<string> ExplicitlyDeclaredContracts
		{
			get { return allDeclaredContracts ?? (allDeclaredContracts = ConfiguredPluggable.ExplicitlyDeclaredContracts.Concat(additionalDeclaredContracts)); }
		}

		public DependenciesBag Dependencies
		{
			get { return ConfiguredPluggable.Dependencies; }
		}

		public Type[] InjectableConstructorArgsTypes
		{
			get { return ConfiguredPluggable.InjectableConstructorArgsTypes; }
		}

		public IInstanceFactory GetFactory()
		{
			return ConfiguredPluggable.GetFactory();
		}

		public IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType)
		{
			return new ConfiguredTypePluggable(() => ConfiguredPluggable.TryGetClosedGenericPluggable(closedGenericPluginType), additionalDeclaredContracts);
		}

		public void DumpDebugInfo(Action<string> writeLine)
		{
			this.DumpMainInfo(writeLine);
			if(configuredPluggable != null)
				configuredPluggable.DumpDebugInfo(l => writeLine("\t" + l));
		}

		public void Dispose()
		{
			DisposeUtils.Dispose(ref configuredPluggable);
		}
	}
}