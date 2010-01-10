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

		private bool TryGetActualArg(Container container, ParameterInfo formalArg, out object actualArg)
		{
			var dep = configuration.Dependencies.SingleOrDefault(d => d.Name == formalArg.Name) ?? DependencyConfigurator.FromAttributes(formalArg);
			return dep.TryGetValue(formalArg.ParameterType, container, out actualArg);
		}

		protected override object TryCreatePluggable(Container container, Type pluginToCreate)
		{
			IDisposable session = container.ConstructionLogger.StartConstruction(InstanceType);
			ConstructorInfo constructorInfo = InstanceType.GetInjectableConstructor(configuration.InjectableConstructorArgsTypes);
			ParameterInfo[] formalArgs = constructorInfo.GetParameters();
			var actualArgs = new object[formalArgs.Length];
			for(int i = 0; i < actualArgs.Length; i++)
			{
				if(!TryGetActualArg(container, formalArgs[i], out actualArgs[i]))
				{
					session.Dispose();
					return null;
				}
			}
			object pluggable = constructorInfo.Invoke(actualArgs);
			session.Dispose();
			return pluggable;
		}
	}
}