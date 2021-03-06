﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	[DebuggerDisplay("CombPluggable {PluggableType}")]
	public class CombinedConfiguredPluggable : IConfiguredPluggable
	{
		private readonly IConfiguredPluggable parent;
		private readonly IConfiguredPluggable child;
		private readonly IContainerConfiguration childConfiguration;
		private IInstanceFactory factory;

		public CombinedConfiguredPluggable(IConfiguredPluggable parent, IConfiguredPluggable child, IContainerConfiguration childConfiguration)
		{
			this.parent = parent;
			this.child = child;
			this.childConfiguration = childConfiguration;
			Debug.Assert(parent.PluggableType == child.PluggableType);
		}

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
			if(factory == parent.GetFactory())
				factory = null;
			else
				DisposeUtils.Dispose(ref factory);
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
			return factory ?? (factory = CreateFactory());
		}

		private IInstanceFactory CreateFactory()
		{
			if(!parent.ReusePolicy.Overridable)
			{
				if(child.ReuseSpecified && parent.ReusePolicy != child.ReusePolicy)
					throw ContainerException.NoLog("Нельзя переопределить политику переиспользования для типа {0}, с унаследованной политикой переиспользования {1}.", parent.PluggableType, parent.ReusePolicy);
				if(child.InitializePluggable != null)
					throw ContainerException.NoLog("Нельзя переопределить инициализатор для типа {0}, с унаследованной политикой переиспользования {1}.", parent.PluggableType, parent.ReusePolicy);
				return parent.GetFactory();
			}
			return parent.GetFactory().CreateByPrototype(this, child.ReusePolicy, child.InitializePluggable, childConfiguration);
		}

		public IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType)
		{
			IConfiguredPluggable closedParent = parent.TryGetClosedGenericPluggable(closedGenericPluginType);
			IConfiguredPluggable closedChild = child.TryGetClosedGenericPluggable(closedGenericPluginType);
			return new CombinedConfiguredPluggable(closedParent, closedChild, childConfiguration);
		}
	}
}