using System;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ByConstructorInstanceFactory : AbstractInstanceFactory
	{
		private readonly IConfiguredPluggable configuration;

		public ByConstructorInstanceFactory(IConfiguredPluggable configuration)
			: this(configuration, configuration.ReusePolicy, configuration.InitializePluggable)
		{
		}

		public ByConstructorInstanceFactory(IConfiguredPluggable configuration, Func<IReuse> reusePolicy, InitializePluggableDelegate<object> initializator)
			: base(configuration.PluggableType, reusePolicy, initializator)
		{
			this.configuration = configuration;
		}

		protected override IInstanceFactory DoCreateByPrototype(Func<IReuse> reusePolicy, InitializePluggableDelegate<object> initializator)
		{
			return new ByConstructorInstanceFactory(configuration, reusePolicy, initializator);
		}

		protected override object TryCreatePluggable(Container container, Type pluginToCreate)
		{
			var session = container.ConstructionLogger.StartConstruction(InstanceType);
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