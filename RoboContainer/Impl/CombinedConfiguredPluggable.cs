using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	[DebuggerDisplay("CombPluggable {PluggableType}")]
	public class CombinedConfiguredPluggable : IConfiguredPluggable
	{
		private readonly IConfiguredPluggable child;
		private readonly IContainerConfiguration childConfiguration;
		private readonly Deferred<IInstanceFactory> factory;
		private readonly IConfiguredPluggable parent;

		public CombinedConfiguredPluggable(IConfiguredPluggable parent, IConfiguredPluggable child,
		                                   IContainerConfiguration childConfiguration)
		{
			this.parent = parent;
			this.child = child;
			this.childConfiguration = childConfiguration;
			factory = new Deferred<IInstanceFactory>(FactoryCreator, FactoryFinalizer);
			Debug.Assert(parent.PluggableType == child.PluggableType);
		}

		#region IConfiguredPluggable Members

		public void DumpDebugInfo(Action<string> writeLine)
		{
			this.DumpMainInfo(writeLine);
			writeLine("\tChild:");
			child.DumpDebugInfo(line => writeLine("\t" + line));
			writeLine("\tParent:");
			parent.DumpDebugInfo(line => writeLine("\t" + line));
		}

		public void Dispose()
		{
			factory.Dispose();
		}

		public Type PluggableType
		{
			get { return parent.PluggableType; }
		}

		public bool Ignored
		{
			get { return parent.Ignored || child.Ignored; }
		}

		public IReusePolicy ReusePolicy
		{
			get { return child.ReuseSpecified ? child.ReusePolicy : parent.ReusePolicy; }
		}

		public bool ReuseSpecified
		{
			get { return child.ReuseSpecified || parent.ReuseSpecified; }
		}

		public InitializePluggableDelegate<object> InitializePluggable
		{
			get { return child.InitializePluggable ?? parent.InitializePluggable; }
		}

		public IEnumerable<string> ExplicitlyDeclaredContracts
		{
			get { return parent.ExplicitlyDeclaredContracts.Union(child.ExplicitlyDeclaredContracts); }
		}

		public DependenciesBag Dependencies
		{
			get { return parent.ReusePolicy.Overridable ? parent.Dependencies.CombineWith(child.Dependencies) : parent.Dependencies; }
		}

		public Type[] InjectableConstructorArgsTypes
		{
			get { return parent.InjectableConstructorArgsTypes ?? child.InjectableConstructorArgsTypes; }
		}

		public IInstanceFactory GetFactory()
		{
			return factory.Get();
		}

		public IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType)
		{
			IConfiguredPluggable closedParent = parent.TryGetClosedGenericPluggable(closedGenericPluginType);
			IConfiguredPluggable closedChild = child.TryGetClosedGenericPluggable(closedGenericPluginType);
			return new CombinedConfiguredPluggable(closedParent, closedChild, childConfiguration);
		}

		#endregion

		private IInstanceFactory FactoryCreator()
		{
			if (!parent.ReusePolicy.Overridable)
			{
				if (child.ReuseSpecified && parent.ReusePolicy != child.ReusePolicy)
					throw ContainerException.NoLog(
						"Нельзя переопределить политику переиспользования для типа {0}, с унаследованной политикой переиспользования {1}.",
						parent.PluggableType, parent.ReusePolicy);
				if (child.InitializePluggable != null)
					throw ContainerException.NoLog(
						"Нельзя переопределить инициализатор для типа {0}, с унаследованной политикой переиспользования {1}.",
						parent.PluggableType, parent.ReusePolicy);
				return parent.GetFactory();
			}
			return parent.GetFactory().CreateByPrototype(this, child.ReusePolicy, child.InitializePluggable, childConfiguration);
		}

		private void FactoryFinalizer(IInstanceFactory instance)
		{
			if (instance != parent.GetFactory())
				DisposeUtils.Dispose(ref instance);
		}
	}
}