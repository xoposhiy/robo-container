using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConfiguredInstancePluggable : IConfiguredPluggable
	{
		private readonly ContractDeclaration[] declaredContracts;
		private object part;

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

		public IReusePolicy ReusePolicy
		{
			get { return new Reuse.Always(); }
		}

		public bool ReuseSpecified
		{
			get { return true; }
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
			if(disp != null) disp.Dispose();
			part = null;
		}
	}
}