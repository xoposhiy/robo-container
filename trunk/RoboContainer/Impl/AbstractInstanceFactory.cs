using System;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public abstract class AbstractInstanceFactory : IInstanceFactory
	{
		private readonly IReusePolicy reusePolicy;
		private readonly InitializePluggableDelegate<object> initializePluggable;
		private readonly IReuseSlot reuseValueSlot;

		protected AbstractInstanceFactory(Type pluggableType, IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializePluggable, IContainerConfiguration configuration)
		{
			reuseValueSlot = reusePolicy.CreateSlot();
			this.reusePolicy = reusePolicy;
			this.initializePluggable = initializePluggable;
			InstanceType = pluggableType;
			Configuration = configuration;
		}

		public Type InstanceType { get; protected set; }
		public IContainerConfiguration Configuration { get; private set; }

		public object TryGetOrCreate(Container not_used, Type typeToCreate)
		{
			if(reuseValueSlot.Value != null)
			{
				Configuration.GetConfiguredLogging().GetLogger().Reused(reuseValueSlot.Value.GetType());
				return reuseValueSlot.Value;
			}
			var container = new Container(Configuration);
			return reuseValueSlot.Value = TryConstructAndLog(container, typeToCreate); // it is ok — result of assignment operator is the right part of assignment (according to C# spec)
		}

		public IInstanceFactory CreateByPrototype(IReusePolicy newReusePolicy, InitializePluggableDelegate<object> newInitializator, IContainerConfiguration configuration)
		{
			if(newReusePolicy != null && !newReusePolicy.Equals(reusePolicy) || newInitializator != null || Configuration != configuration)
				return DoCreateByPrototype(newReusePolicy, CombineInitializators(newInitializator, initializePluggable), configuration);
			return this;
		}

		public void Dispose()
		{
			reuseValueSlot.Dispose();
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

		protected abstract IInstanceFactory DoCreateByPrototype(IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializator, IContainerConfiguration configuration);

		private object TryConstructAndLog(Container container, Type typeToCreate)
		{
			IConstructionLogger constructionLog = container.ConstructionLogger;
			object result = TryConstruct(container, typeToCreate);
			if(result == null) constructionLog.ConstructionFailed(InstanceType);
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
	}
}