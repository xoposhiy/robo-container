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
		public object TryGetOrCreate(IConstructionLogger logger, Type typeToCreate, string[] requiredContracts, Func<object, object> initializeJustCreatedObject)
		{
			bool createdNew;
			var result = reuseValueSlot.GetOrCreate(() => TryConstruct(logger, typeToCreate, requiredContracts, initializeJustCreatedObject), out createdNew);
			if (!createdNew)
				logger.Reused(result);
			return result;
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
		private object TryConstruct(IConstructionLogger logger, Type typeToCreate, string[] requiredContracts, Func<object, object> initializeJustCreatedObject)
		{
			var container = new Container(Configuration);
			object constructed = TryCreatePluggable(container, typeToCreate, requiredContracts, initializeJustCreatedObject);
			if(constructed == null) return null;
			var initializablePluggable = constructed as IInitializablePluggable;
			if(initializablePluggable != null) initializablePluggable.Initialize(container);
			var result = initializePluggable != null ? initializePluggable(constructed, container) : constructed;
			if(result != null) logger.Constructed(result.GetType());
			else logger.ConstructionFailed(InstanceType);
			return result;
		}

		[CanBeNull]
		protected abstract object TryCreatePluggable(Container container, Type pluginToCreate, string[] requiredContracts, Func<object, object> initializeJustCreatedObject);
	}
}