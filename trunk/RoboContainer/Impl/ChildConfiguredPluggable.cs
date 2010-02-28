using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ChildConfiguredPluggable : IConfiguredPluggable
	{
		private readonly IConfiguredPluggable parent;
		private readonly IContainerConfiguration configuration;
		private IInstanceFactory factory;

		public ChildConfiguredPluggable(IConfiguredPluggable parent, IContainerConfiguration configuration)
		{
			this.parent = parent;
			this.configuration = configuration;
		}

		public void Dispose()
		{
			DisposeUtils.Dispose(ref factory);
		}

		public Type PluggableType
		{
			get { return parent.PluggableType; }
		}

		public bool Ignored
		{
			get { return parent.Ignored; }
		}

		public IReusePolicy ReusePolicy
		{
			get { return parent.ReusePolicy; }
		}

		public bool ReuseSpecified
		{
			get { return parent.ReuseSpecified; }
		}

		public InitializePluggableDelegate<object> InitializePluggable
		{
			get { return parent.InitializePluggable; }
		}

		public IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts
		{
			get { return parent.ExplicitlyDeclaredContracts; }
		}

		public IEnumerable<IConfiguredDependency> Dependencies
		{
			get { return parent.Dependencies; }
		}

		public Type[] InjectableConstructorArgsTypes
		{
			get { return parent.InjectableConstructorArgsTypes; }
		}

		public IInstanceFactory GetFactory()
		{
			if(parent.ReusePolicy.ReusableFromChildContainer) return parent.GetFactory();
			return factory ?? (factory = parent.GetFactory().CreateByPrototype(parent.ReusePolicy, parent.InitializePluggable, configuration));
		}

		public IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType)
		{
			return new ChildConfiguredPluggable(parent.TryGetClosedGenericPluggable(closedGenericPluginType), configuration);
		}
	}
}