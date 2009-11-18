using System;

namespace RoboContainer.Impl
{
	internal class DelegateInstanceFactory : BaseInstanceFactory
	{
		private readonly CreatePluggableDelegate createPluggable;

		public DelegateInstanceFactory(InstanceLifetime scope, InitializePluggableDelegate initializePluggable,
		                               CreatePluggableDelegate createPluggable)
			: base(null, scope, initializePluggable)
		{
			this.createPluggable = createPluggable;
		}

		protected override object CreatePluggable(Container container, Type pluginToCreate)
		{
			return createPluggable(container, pluginToCreate);
		}
	}
}