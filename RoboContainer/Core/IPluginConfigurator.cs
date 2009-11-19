using System;

namespace RoboContainer.Core
{
	public delegate TPlugin CreatePluggableDelegate<TPlugin>(Container container, Type pluginType);

	public interface IGenericPluginConfigurator<TPlugin, TPluginConfigurator>
	{
		TPluginConfigurator Ignore<TPluggable>();
		TPluginConfigurator Ignore(params Type[] pluggableTypes);
		TPluginConfigurator PluggableIs<TPluggable>();
		TPluginConfigurator PluggableIs(Type pluggableType);
		TPluginConfigurator SetScope(InstanceLifetime lifetime);
		TPluginConfigurator RequireContracts(params ContractRequirement[] requiredContracts);
		TPluginConfigurator CreatePluggableBy(CreatePluggableDelegate<TPlugin> createPluggable);
		TPluginConfigurator InitializeWith(InitializePluggableDelegate<TPlugin> initializePluggable);
		TPluginConfigurator InitializeWith(Action<TPlugin> initializePlugin);
	}

	public interface IPluginConfigurator : IGenericPluginConfigurator<object, IPluginConfigurator>
	{
		IPluginConfigurator<TPlugin> TypedConfigurator<TPlugin>();
	}

	public interface IPluginConfigurator<TPlugin> : IGenericPluginConfigurator<TPlugin, IPluginConfigurator<TPlugin>>
	{
	}
}