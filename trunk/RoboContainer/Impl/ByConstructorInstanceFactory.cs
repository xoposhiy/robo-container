using System;
using System.Linq;
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

		public ByConstructorInstanceFactory(IConfiguredPluggable pluggable, IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializator, IContainerConfiguration configuration)
			: base(pluggable.PluggableType, reusePolicy, initializator, configuration)
		{
			this.pluggable = pluggable;
		}

		protected override IInstanceFactory DoCreateByPrototype(IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializator, IContainerConfiguration configuration)
		{
			return new ByConstructorInstanceFactory(pluggable, reusePolicy, initializator, configuration);
		}

		private bool TryGetActualArg(Container container, ParameterInfo formalArg, out object actualArg)
		{
			var dep = pluggable.Dependencies.SingleOrDefault(d => d.Name == formalArg.Name) ?? DependencyConfigurator.FromAttributes(formalArg);
			return dep.TryGetValue(formalArg.ParameterType, container, out actualArg);
		}

		protected override object TryCreatePluggable(Container not_used, Type pluginToCreate)
		{
			IDisposable session = Configuration.GetConfiguredLogging().GetLogger().StartConstruction(InstanceType);
			ConstructorInfo constructorInfo = InstanceType.GetInjectableConstructor(pluggable.InjectableConstructorArgsTypes);
			ParameterInfo[] formalArgs = constructorInfo.GetParameters();
			var actualArgs = new object[formalArgs.Length];
			var container = new Container(Configuration);
			for(int i = 0; i < actualArgs.Length; i++)
			{
				if(!TryGetActualArg(container, formalArgs[i], out actualArgs[i]))
				{
					session.Dispose();
					return null;
				}
			}
			object createdPluggable = constructorInfo.Invoke(actualArgs);
			session.Dispose();
			return createdPluggable;
		}
	}
}