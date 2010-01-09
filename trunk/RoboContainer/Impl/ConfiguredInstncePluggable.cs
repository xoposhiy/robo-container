using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConfiguredInstancePluggable : IConfiguredPluggable
	{
		private object part;
		private readonly ContractDeclaration[] declaredContracts;

		public ConfiguredInstancePluggable(object part, ContractDeclaration[] declaredContracts)
		{
			this.part = part;
			this.declaredContracts = declaredContracts;
		}

		public Type PluggableType
		{
			get { return part.GetType(); }
		}

		public bool Ignored
		{
			get { return false; }
		}

		public Func<IReuse> ReusePolicy
		{
			get { return ReusePolicies.Always; }
		}

		public InitializePluggableDelegate<object> InitializePluggable
		{
			get { return null; }
		}

		public IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts
		{
			get { return declaredContracts; }
		}

		public IEnumerable<IConfiguredDependency> Dependencies
		{
			get { yield break; }
		}

		public Type[] InjectableConstructorArgsTypes
		{
			get { return null; }
		}

		public IInstanceFactory GetFactory()
		{
			return new ConstantInstanceFactory(part);
		}

		public IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType)
		{
			return null;
		}

		public void Dispose()
		{
			var disp = part as IDisposable;
			if (disp != null) disp.Dispose();
			part = null;
		}
	}
}