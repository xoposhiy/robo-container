﻿using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IInstanceFactory
	{
		Type InstanceType { get; }
		object TryGetOrCreate(Container container, Type typeToCreate);
		IInstanceFactory CreateByPrototype(Func<IReuse> reusePolicy, InitializePluggableDelegate<object> initializator);
	}
}