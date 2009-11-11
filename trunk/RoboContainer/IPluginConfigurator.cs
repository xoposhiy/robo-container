using System;

namespace RoboContainer
{
	public delegate object CreatePluggableDelegate(Container container, Type pluginType);
	public delegate TPlugin CreatePluggableDelegate<TPlugin>(Container container, Type pluginType);

	public interface IPluginConfigurator
	{
		IPluginConfigurator Ignore<TPluggable>();
		IPluginConfigurator Ignore(params Type[] pluggableTypes);
		IPluginConfigurator PluggableIs<TPluggable>();
		IPluginConfigurator PluggableIs(Type pluggableType);
		IPluginConfigurator SetScope(InstanceLifetime lifetime);
		IPluginConfigurator CreatePluggableBy(CreatePluggableDelegate createPluggable);
		IPluginConfigurator EnrichWith(EnrichPluggableDelegate enrichPluggable);
		IPluginConfigurator EnrichWith(Action<object> enrichPlugin);
		IPluginConfigurator<TPlugin> TypedConfigurator<TPlugin>();
	}

	public interface IPluginConfigurator<TPlugin>
	{
		IPluginConfigurator<TPlugin> Ignore<TPluggable>();
		IPluginConfigurator<TPlugin> Ignore(params Type[] pluggableTypes);
		IPluginConfigurator<TPlugin> PluggableIs<TPluggable>();
		IPluginConfigurator<TPlugin> PluggableIs(Type pluggableType);
		IPluginConfigurator<TPlugin> SetScope(InstanceLifetime lifetime);
		IPluginConfigurator<TPlugin> CreatePluggableBy(CreatePluggableDelegate<TPlugin> createPluggable);
		IPluginConfigurator<TPlugin> EnrichWith(EnrichPluggableDelegate<TPlugin> enrichPluggable);
		IPluginConfigurator<TPlugin> EnrichWith(Action<TPlugin> enrichPlugin);
	}
}