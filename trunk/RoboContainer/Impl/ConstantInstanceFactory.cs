using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConstantInstanceFactory : IInstanceFactory
	{
		private object instance;

		public ConstantInstanceFactory(object instance)
		{
			this.instance = instance;
		}

		public Type InstanceType
		{
			get { return instance.GetType(); }
		}

		public object TryGetOrCreate(Container container, Type typeToCreate)
		{
			container.ConstructionLogger.Reused(instance.GetType());
			return instance;
		}

		public IInstanceFactory CreateByPrototype(Func<IReuse> reusePolicy, InitializePluggableDelegate<object> initializator)
		{
			return this;
		}

		public void Dispose()
		{
			var disp = instance as IDisposable;
			if (disp != null) disp.Dispose();
			instance = null;
		}
	}
}