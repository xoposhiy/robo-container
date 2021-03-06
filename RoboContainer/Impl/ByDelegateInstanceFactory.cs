﻿using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ByDelegateInstanceFactory : AbstractInstanceFactory
	{
		private readonly CreatePluggableDelegate<object> createPluggable;

		public ByDelegateInstanceFactory(
			IReusePolicy reusePolicy,
			InitializePluggableDelegate<object> initializePluggable,
			CreatePluggableDelegate<object> createPluggable, 
			IContainerConfiguration configuration)
			: base(null, reusePolicy, initializePluggable, configuration)
		{
			this.createPluggable = createPluggable;
		}

		protected override IInstanceFactory DoCreateByPrototype(IConfiguredPluggable pluggable, IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializator, IContainerConfiguration configuration)
		{
			return new ByDelegateInstanceFactory(reusePolicy, initializator, createPluggable, configuration);
		}

		[CanBeNull]
		protected override object TryCreatePluggable(Container container, Type pluginToCreate, string[] requiredContracts, Func<object, object> initializeJustCreatedObject)
		{
			var pluggable = createPluggable(container, pluginToCreate, requiredContracts);
			if (pluggable != null) pluggable = initializeJustCreatedObject(pluggable);
			return pluggable;
		}
	}
}