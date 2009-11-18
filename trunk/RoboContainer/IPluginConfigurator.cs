using System;

namespace RoboContainer
{
	public delegate object CreatePluggableDelegate(Container container, Type pluginType);
	public delegate TPlugin CreatePluggableDelegate<TPlugin>(Container container, Type pluginType);

	public interface IGenericPluginConfigurator<TPC>
	{
		TPC Ignore<TPluggable>();
		TPC Ignore(params Type[] pluggableTypes);
		TPC PluggableIs<TPluggable>();
		TPC PluggableIs(Type pluggableType);
		TPC SetScope(InstanceLifetime lifetime);
		TPC RequireContracts(params string[] requiredContracts);
	}
	
	public interface IPluginConfigurator : IGenericPluginConfigurator<IPluginConfigurator>
	{
		IPluginConfigurator CreatePluggableBy(CreatePluggableDelegate createPluggable);
		IPluginConfigurator InitializeWith(InitializePluggableDelegate initializePluggable);
		IPluginConfigurator InitializeWith(Action<object> initializePlugin);
		IPluginConfigurator<TPlugin> TypedConfigurator<TPlugin>();
	}

	public interface IPluginConfigurator<TPlugin> : IGenericPluginConfigurator<IPluginConfigurator<TPlugin>>
	{
		IPluginConfigurator<TPlugin> CreatePluggableBy(CreatePluggableDelegate<TPlugin> createPluggable);
		IPluginConfigurator<TPlugin> InitializeWith(InitializePluggableDelegate<TPlugin> initializePluggable);
		IPluginConfigurator<TPlugin> InitializeWith(Action<TPlugin> initializePlugin);
	}
}