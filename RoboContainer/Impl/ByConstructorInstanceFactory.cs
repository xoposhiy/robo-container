using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ByConstructorInstanceFactory : AbstractInstanceFactory
	{
		private readonly Deferred<ConstructorInvoker> bestConstructor;
		private readonly IConfiguredPluggable pluggable;

		public ByConstructorInstanceFactory(IConfiguredPluggable pluggable, IContainerConfiguration configuration)
			: this(pluggable, pluggable.ReusePolicy, pluggable.InitializePluggable, configuration)
		{
		}

		private ByConstructorInstanceFactory(IConfiguredPluggable pluggable, IReusePolicy reusePolicy,
		                                     InitializePluggableDelegate<object> initializator,
		                                     IContainerConfiguration configuration)
			: base(pluggable.PluggableType, reusePolicy, initializator, configuration)
		{
			bestConstructor = new Deferred<ConstructorInvoker>(GetBestConstructor);
			this.pluggable = pluggable;
		}

		protected override IInstanceFactory DoCreateByPrototype(IConfiguredPluggable aPluggable, IReusePolicy reusePolicy,
		                                                        InitializePluggableDelegate<object> initializator,
		                                                        IContainerConfiguration configuration)
		{
			return new ByConstructorInstanceFactory(aPluggable, reusePolicy, initializator, configuration);
		}

		protected override object TryCreatePluggable(Container container, Type pluginToCreate, string[] requiredContracts,
		                                             Func<object, object> initializeJustCreatedObject)
		{
			using (Configuration.GetConfiguredLogging().GetLogger().StartConstruction(InstanceType))
			{
				ConstructorInvoker bestConstructorInvoker = bestConstructor.Get();
				object[] actualArgs = pluggable.Dependencies.TryGetActualArgs(bestConstructorInvoker.ConstructorInfo, container);
				if (actualArgs == null) return null;
				try
				{
					return initializeJustCreatedObject(bestConstructorInvoker.Invoke(actualArgs));
				}
				catch (TargetInvocationException e)
				{
					if (e.InnerException != null)
						throw ContainerException.Wrap(e.InnerException);
					throw;
				}
			}
		}

		private ConstructorInvoker GetBestConstructor()
		{
			IEnumerable<ConstructorInfo> constructors =
				InstanceType.GetInjectableConstructors(pluggable.InjectableConstructorArgsTypes);
			ConstructorInfo result = constructors.First();
			foreach (ConstructorInfo c in constructors)
				if (c.GetParameters().Length > result.GetParameters().Length) result = c;
			return new ConstructorInvoker(result);
		}
	}
}