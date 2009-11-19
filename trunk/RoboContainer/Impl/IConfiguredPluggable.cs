﻿using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredPluggable
	{
		Type PluggableType { get; }
		bool Ignored { get; }
		InstanceLifetime Scope { get; }
		InitializePluggableDelegate<object> InitializePluggable { get; }
		IEnumerable<DeclaredContract> Contracts { get; }
		IEnumerable<IConfiguredDependency> Dependencies { get; }
		IInstanceFactory GetFactory();
	}
}