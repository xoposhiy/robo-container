﻿using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IInstanceFactory : IDisposable
	{
		Type InstanceType { get; }
		[CanBeNull]
		object TryGetOrCreate(IConstructionLogger logger, Type typeToCreate);
		IInstanceFactory CreateByPrototype(IConfiguredPluggable newPluggable, IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializator, IContainerConfiguration configuration);
	}
}