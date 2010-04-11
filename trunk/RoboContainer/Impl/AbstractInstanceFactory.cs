using System;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public abstract class AbstractInstanceFactory : IInstanceFactory
	{
		private readonly InitializePluggableDelegate<object> initializePluggable;
		private readonly IReuseSlot reuseValueSlot;

		protected AbstractInstanceFactory(Type pluggableType, IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializePluggable, IContainerConfiguration configuration)
		{
			reuseValueSlot = reusePolicy.CreateSlot();
			this.initializePluggable = initializePluggable;
			InstanceType = pluggableType;
			Configuration = configuration;
		}

		public Type InstanceType { get; private set; }
		public IContainerConfiguration Configuration { get; private set; }

		[CanBeNull]
		public object TryGetOrCreate(IConstructionLogger logger, Type typeToCreate, ContractRequirement[] requiredContracts, out bool justCreated)
		{
			if(reuseValueSlot.Value != null)
			{
				logger.Reused(reuseValueSlot.Value.GetType());
				justCreated = false;
				return reuseValueSlot.Value;
			}
			var result = TryConstructAndLog(logger, typeToCreate, requiredContracts);
			justCreated = result != null;
			return reuseValueSlot.Value = result;
		}

		public IInstanceFactory CreateByPrototype(IConfiguredPluggable newPluggable, IReusePolicy newReusePolicy, InitializePluggableDelegate<object> newInitializator, IContainerConfiguration configuration)
		{
			return DoCreateByPrototype(newPluggable, newReusePolicy, newInitializator, configuration);
		}

		public void Dispose()
		{
			reuseValueSlot.Dispose();
		}

		protected abstract IInstanceFactory DoCreateByPrototype(IConfiguredPluggable pluggable, IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializator, IContainerConfiguration configuration);

		[CanBeNull]
		private object TryConstructAndLog(IConstructionLogger logger, Type typeToCreate, ContractRequirement[] requiredContracts)
		{
			object result = TryConstruct(typeToCreate, requiredContracts);
			if(result == null) logger.ConstructionFailed(InstanceType);
			else logger.Constructed(result.GetType());
			return result;
		}

		[CanBeNull]
		private object TryConstruct(Type typeToCreate, ContractRequirement[] requiredContracts)
		{
			var container = new Container(Configuration);
			object constructed = TryCreatePluggable(container, typeToCreate, requiredContracts);
			if(constructed == null) return null;
			var initializablePluggable = constructed as IInitializablePluggable;
			if(initializablePluggable != null) initializablePluggable.Initialize(container);
			return initializePluggable != null ? initializePluggable(constructed, container) : constructed;
		}

		[CanBeNull]
		protected abstract object TryCreatePluggable(Container container, Type pluginToCreate, ContractRequirement[] requiredContracts);
	}
}