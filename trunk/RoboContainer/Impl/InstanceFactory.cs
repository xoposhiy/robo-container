﻿using System;
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

		public object TryGetOrCreate(Container container, Type typeToCreate)
		{
			if(o == null || scope == InstanceLifetime.PerRequest)
			{
				o = TryConstruct(container, typeToCreate);
				if (o == null) container.LastConstructionLog.ConstructionFailed(InstanceType);
				else container.LastConstructionLog.Constructed(o.GetType());
			}
			else
				container.LastConstructionLog.Reused(o.GetType());
			return o;
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

	internal class InstanceFactory : BaseInstanceFactory
	{
		private readonly IConfiguredPluggable configuration;

		public InstanceFactory(IConfiguredPluggable configuration)
			: base(configuration.PluggableType, configuration.Scope, configuration.InitializePluggable)
		{
			this.configuration = configuration;
		}

		protected override object TryCreatePluggable(Container container, Type pluginToCreate)
		{
			var session = container.LastConstructionLog.StartConstruction(InstanceType);
			ConstructorInfo constructorInfo = InstanceType.GetInjectableConstructor(configuration.InjectableConstructorArgsTypes);
			var formalArgs = constructorInfo.GetParameters();
			var actualArgs = new object[formalArgs.Length];
			for(int i=0; i<actualArgs.Length; i++)
			{
				object actualArg;
				if (!configuration.Dependencies.ElementAt(i).TryGetValue(formalArgs[i], container, out actualArg))
				{
					session.Dispose();
					return null;
				}
				actualArgs[i] = actualArg;
			}
			var pluggable = constructorInfo.Invoke(actualArgs);
			session.Dispose();
			return pluggable;
		}
	}
}