using System;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public abstract class AbstractInstanceFactory : IInstanceFactory
	{
		private readonly Func<IReuse> createReuseSlot;
		private readonly InitializePluggableDelegate<object> initializePluggable;
		private readonly IReuse reuseValueSlot;

		protected AbstractInstanceFactory(Type pluggableType, Func<IReuse> createReuseSlot, InitializePluggableDelegate<object> initializePluggable)
		{
			reuseValueSlot = createReuseSlot();
			this.createReuseSlot = createReuseSlot;
			this.initializePluggable = initializePluggable;
			InstanceType = pluggableType;
		}

		public Type InstanceType { get; protected set; }

		public object TryGetOrCreate(Container container, Type typeToCreate)
		{
			if (reuseValueSlot.Value != null)
			{
				container.ConstructionLogger.Reused(reuseValueSlot.Value.GetType());
				return reuseValueSlot.Value;
			}
			return reuseValueSlot.Value = TryConstructAndLog(container, typeToCreate); // it is ok — result of assignment operator is the right part of assignment (according to C# spec)
		}

		public IInstanceFactory CreateByPrototype(Func<IReuse> reusePolicy, InitializePluggableDelegate<object> initializator)
		{
			if(reusePolicy != null && reusePolicy != createReuseSlot || initializator != null)
				return DoCreateByPrototype(reusePolicy, CombineInitializators(initializator, initializePluggable));
			return this;
		}

		private static InitializePluggableDelegate<object> CombineInitializators(InitializePluggableDelegate<object> pluginInitializator, InitializePluggableDelegate<object> pluggableInitializator)
		{
			if(pluggableInitializator == null || pluginInitializator == null)
				return pluggableInitializator ?? pluginInitializator;
			return
				(pluggable, container) =>
					{
						pluggableInitializator(pluggable, container);
						pluginInitializator(pluggable, container);
						return pluggable;
					};
		}

		protected abstract IInstanceFactory DoCreateByPrototype(Func<IReuse> reusePolicy, InitializePluggableDelegate<object> initializator);

		private object TryConstructAndLog(Container container, Type typeToCreate)
		{
			var constructionLog = container.ConstructionLogger;
			object result = TryConstruct(container, typeToCreate);
			if (result == null) constructionLog.ConstructionFailed(InstanceType);
			else constructionLog.Constructed(result.GetType());
			return result;
		}

		private object TryConstruct(Container container, Type typeToCreate)
		{
			object constructed = TryCreatePluggable(container, typeToCreate);
			if(constructed == null) return null;
			var initializablePluggable = constructed as IInitializablePluggable;
			if(initializablePluggable != null) initializablePluggable.Initialize(container);
			return initializePluggable != null ? initializePluggable(constructed, container) : constructed;
		}

		protected abstract object TryCreatePluggable(Container container, Type pluginToCreate);
		
		public void Dispose()
		{
			reuseValueSlot.Dispose();
		}
	}
}