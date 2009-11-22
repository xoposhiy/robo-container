﻿using System;

namespace RoboContainer.Core
{
	public delegate TPlugin CreatePluggableDelegate<TPlugin>(Container container, Type pluginType);

	public interface IGenericPluginConfigurator<TPlugin, TSelf>
	{
		TSelf Ignore<TPluggable>();
		TSelf Ignore(params Type[] pluggableTypes);
		TSelf UsePluggable<TPluggable>();
		TSelf UsePluggable(Type pluggableType);
		TSelf Use(TPlugin pluggable);
		TSelf UseAlso(TPlugin pluggable);
		TSelf ReusePluggable(ReusePolicy reusePolicy);
		TSelf ReusePluggable<TReuse>() where TReuse : IReuse, new();
		TSelf RequireContracts(params ContractRequirement[] requiredContracts);
		TSelf CreatePluggableBy(CreatePluggableDelegate<TPlugin> createPluggable);
		TSelf InitializeWith(InitializePluggableDelegate<TPlugin> initializePluggable);
		TSelf InitializeWith(Action<TPlugin> initializePlugin);
	}

	public interface IPluginConfigurator : IGenericPluginConfigurator<object, IPluginConfigurator>
	{
		IPluginConfigurator<TPlugin> TypedConfigurator<TPlugin>();
	}

	public interface IPluginConfigurator<TPlugin> : IGenericPluginConfigurator<TPlugin, IPluginConfigurator<TPlugin>>
	{
	}
}