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
		private readonly Deferred<IEnumerable<string>> allDeclaredContracts;
		private readonly Deferred<IConfiguredPluggable> configuredPluggable;

		public ConfiguredTypePluggable(Func<IConfiguredPluggable> configuredPluggableProvider,
		                               string[] additionalDeclaredContracts)
		{
			this.additionalDeclaredContracts = additionalDeclaredContracts;
			configuredPluggable = new Deferred<IConfiguredPluggable>(configuredPluggableProvider,
			                                                         ConfiguredPluggableFinalizer);
			allDeclaredContracts =
				new Deferred<IEnumerable<string>>(
					() => ConfiguredPluggable.ExplicitlyDeclaredContracts.Concat(additionalDeclaredContracts));
		}

		private IConfiguredPluggable ConfiguredPluggable
		{
			get { return configuredPluggable.Get(); }
		}

		#region IConfiguredPluggable Members

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
			get { return allDeclaredContracts.Get(); }
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
			return new ConfiguredTypePluggable(() => ConfiguredPluggable.TryGetClosedGenericPluggable(closedGenericPluginType),
			                                   additionalDeclaredContracts);
		}

		public void DumpDebugInfo(Action<string> writeLine)
		{
			this.DumpMainInfo(writeLine);
			configuredPluggable.IfCreated(p => p.DumpDebugInfo(l => writeLine("\t" + l)));
		}

		public void Dispose()
		{
			configuredPluggable.Dispose();
		}

		#endregion

		private static void ConfiguredPluggableFinalizer(IConfiguredPluggable instance)
		{
			DisposeUtils.Dispose(ref instance);
		}
	}
}