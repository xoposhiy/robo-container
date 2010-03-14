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
		public object TryGetOrCreate(IConstructionLogger logger, Type typeToCreate)
		{
			if(reuseValueSlot.Value != null)
			{
				logger.Reused(reuseValueSlot.Value.GetType());
				return reuseValueSlot.Value;
			}
			return reuseValueSlot.Value = TryConstructAndLog(logger, typeToCreate); // it is ok — result of assignment operator is the right part of assignment (according to C# spec)
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
		private object TryConstructAndLog(IConstructionLogger logger, Type typeToCreate)
		{
			object result = TryConstruct(typeToCreate);
			if(result == null) logger.ConstructionFailed(InstanceType);
			else logger.Constructed(result.GetType());
			return result;
		}

		[CanBeNull]
		private object TryConstruct(Type typeToCreate)
		{
			var container = new Container(Configuration);
			object constructed = TryCreatePluggable(container, typeToCreate);
			if(constructed == null) return null;
			var initializablePluggable = constructed as IInitializablePluggable;
			if(initializablePluggable != null) initializablePluggable.Initialize(container);
			return initializePluggable != null ? initializePluggable(constructed, container) : constructed;
		}

		[CanBeNull]
		protected abstract object TryCreatePluggable(Container container, Type pluginToCreate);
	}
}