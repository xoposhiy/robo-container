using System;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	internal abstract class BaseInstanceFactory : IInstanceFactory
	{
		private readonly InitializePluggableDelegate<object> initializePluggable;
		private readonly InstanceLifetime scope;
		private object o;

		protected BaseInstanceFactory(Type pluggableType, InstanceLifetime scope, InitializePluggableDelegate<object> initializePluggable)
		{
			this.scope = scope;
			this.initializePluggable = initializePluggable;
			InstanceType = pluggableType;
		}

		public Type InstanceType { get; protected set; }

		public object GetOrCreate(Container container, Type typeToCreate)
		{
			if (o == null || scope == InstanceLifetime.PerRequest)
				container.LastConstructionLog.Constructed((o = Construct(container, typeToCreate)).GetType());
			else
				container.LastConstructionLog.Reused(o.GetType());
			return o;
		}

		private object Construct(Container container, Type typeToCreate)
		{
			object constructed = CreatePluggable(container, typeToCreate);
			var initializablePluggable = constructed as IInitializablePluggable;
			if (initializablePluggable != null) initializablePluggable.Initialize(container);
			return initializePluggable != null ? initializePluggable(constructed, container) : constructed;
		}

		protected abstract object CreatePluggable(Container container, Type pluginToCreate);
	}

	internal class InstanceFactory : BaseInstanceFactory
	{
		private readonly IConfiguredPluggable configuration;

		public InstanceFactory(IConfiguredPluggable configuration)
			: base(configuration.PluggableType, configuration.Scope, configuration.InitializePluggable)
		{
			this.configuration = configuration;
		}

		protected override object CreatePluggable(Container container, Type pluginToCreate)
		{
			ConstructorInfo constructorInfo = InstanceType.GetInjectableConstructor();
			object[] arguments =
				constructorInfo.GetParameters()
					.Select(
					(p, i) =>
					container.Get(
						p.ParameterType,
						configuration.Dependencies.ElementAt(i).Contracts.ToArray()
						)).ToArray();
			return constructorInfo.Invoke(arguments);
		}
	}
}