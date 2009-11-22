using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	internal class DelegateInstanceFactory : BaseInstanceFactory
	{
		private readonly CreatePluggableDelegate<object> createPluggable;

		public DelegateInstanceFactory(LifetimeScope scope, InitializePluggableDelegate<object> initializePluggable,
		                               CreatePluggableDelegate<object> createPluggable)
			: base(null, scope, initializePluggable)
		{
			this.createPluggable = createPluggable;
		}

		protected override object TryCreatePluggable(Container container, Type pluginToCreate)
		{
			return createPluggable(container, pluginToCreate);
		}
	}
}