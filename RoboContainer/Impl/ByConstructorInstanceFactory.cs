using System;
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
				ConstructorInfo constructorInfo = InstanceType.GetInjectableConstructor(pluggable.InjectableConstructorArgsTypes);
				var actualArgs = pluggable.Dependencies.TryGetActualArgs(constructorInfo, container);
				if(actualArgs == null) return null;
				return constructorInfo.Invoke(actualArgs);
			}
		}
	}
}