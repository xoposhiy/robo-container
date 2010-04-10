using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ByConstructorInstanceFactory : AbstractInstanceFactory
	{
		private readonly IConfiguredPluggable pluggable;

		public ByConstructorInstanceFactory(IConfiguredPluggable pluggable, IContainerConfiguration configuration)
			: this(pluggable, pluggable.ReusePolicy, pluggable.InitializePluggable, configuration)
		{
		}

		private ByConstructorInstanceFactory(IConfiguredPluggable pluggable, IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializator, IContainerConfiguration configuration)
			: base(pluggable.PluggableType, reusePolicy, initializator, configuration)
		{
			this.pluggable = pluggable;
		}

		protected override IInstanceFactory DoCreateByPrototype(IConfiguredPluggable aPluggable, IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializator, IContainerConfiguration configuration)
		{
			return new ByConstructorInstanceFactory(aPluggable, reusePolicy, initializator, configuration);
		}

		protected override object TryCreatePluggable(Container container, Type pluginToCreate, ContractRequirement[] requiredContracts)
		{
			using(Configuration.GetConfiguredLogging().GetLogger().StartConstruction(InstanceType))
			{
				IEnumerable<ConstructorInfo> constructors =
					InstanceType.GetInjectableConstructors(pluggable.InjectableConstructorArgsTypes);
				var bestConstructor = constructors.First();
				foreach(var c in constructors)
					if(c.GetParameters().Length > bestConstructor.GetParameters().Length) bestConstructor = c;
				var actualArgs = pluggable.Dependencies.TryGetActualArgs(bestConstructor, container);
				if(actualArgs == null) return null;
				try
				{
					return bestConstructor.Invoke(actualArgs);
				}
				catch(TargetInvocationException e)
				{
					if(e.InnerException != null) throw ContainerException.NoLog(e.InnerException.Message);
					throw;
				}
			}
		}
	}
}