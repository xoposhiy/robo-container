using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ByDelegateInstanceFactory : AbstractInstanceFactory
	{
		private readonly CreatePluggableDelegate<object> createPluggable;

		public ByDelegateInstanceFactory(
			Func<IReuse> scope, 
			InitializePluggableDelegate<object> initializePluggable,
			CreatePluggableDelegate<object> createPluggable)
			: base(null, scope, initializePluggable)
		{
			this.createPluggable = createPluggable;
		}

		protected override IInstanceFactory DoCreateByPrototype(Func<IReuse> reusePolicy, InitializePluggableDelegate<object> initializator)
		{
			return new ByDelegateInstanceFactory(reusePolicy, initializator, createPluggable);
		}

		protected override object TryCreatePluggable(Container container, Type pluginToCreate)
		{
			return createPluggable(container, pluginToCreate);
		}
	}
}