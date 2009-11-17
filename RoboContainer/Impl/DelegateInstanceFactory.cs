using System;

namespace RoboContainer.Impl
{
	internal class DelegateInstanceFactory : BaseInstanceFactory
	{
		private readonly CreatePluggableDelegate createPluggable;

		public DelegateInstanceFactory(InstanceLifetime scope, EnrichPluggableDelegate enrichPluggable,
		                               CreatePluggableDelegate createPluggable)
			: base(null, scope, enrichPluggable)
		{
			this.createPluggable = createPluggable;
		}

		protected override object CreatePluggable(Container container, Type typeToCreate)
		{
			return createPluggable(container, typeToCreate);
		}
	}
}