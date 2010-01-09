using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConfiguredTypePluggable : IConfiguredPluggable
	{
		private readonly ContractDeclaration[] additionalDeclaredContracts;
		private readonly Func<IConfiguredPluggable> configuredPluggableProvider;
		private IConfiguredPluggable configuredPluggable;
		private IEnumerable<ContractDeclaration> declaredContracts;

		public ConfiguredTypePluggable(Func<IConfiguredPluggable> configuredPluggableProvider, ContractDeclaration[] additionalDeclaredContracts)
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

		public Func<IReuse> ReusePolicy
		{
			get { return ConfiguredPluggable.ReusePolicy; }
		}

		public InitializePluggableDelegate<object> InitializePluggable
		{
			get { return ConfiguredPluggable.InitializePluggable; }
		}

		public IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts
		{
			get { return declaredContracts ?? (declaredContracts = ConfiguredPluggable.ExplicitlyDeclaredContracts.Concat(additionalDeclaredContracts)); }
		}

		public IEnumerable<IConfiguredDependency> Dependencies
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

		public void Dispose()
		{
			DisposeUtils.Dispose(ref configuredPluggable);
		}
	}
}