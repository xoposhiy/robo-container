using System;

namespace RoboContainer.Core
{
	public delegate TPlugin CreatePluggableDelegate<TPlugin>(Container container, Type pluginType);


	public interface IGenericPluginConfigurator<TPlugin, TSelf>
	{
		TSelf UsePluggable<TPluggable>(params ContractDeclaration[] declaredContracts) where TPluggable : TPlugin;
		TSelf UsePluggable(Type pluggableType, params ContractDeclaration[] declaredContracts);
		TSelf UseInstance(TPlugin instance, params ContractDeclaration[] declaredContracts);
		TSelf UsePluggableCreatedBy(CreatePluggableDelegate<TPlugin> createPluggable);
		TSelf UseOtherPluggablesToo();
		TSelf DontUse<TPluggable>() where TPluggable : TPlugin;
		TSelf DontUse(params Type[] pluggableTypes);
		TSelf ReusePluggable(ReusePolicy reusePolicy);
		TSelf ReusePluggable<TReuse>() where TReuse : IReuse, new();
		TSelf RequireContracts(params ContractRequirement[] requiredContracts);
		TSelf SetInitializer(InitializePluggableDelegate<TPlugin> initializePluggable);
		TSelf SetInitializer(Action<TPlugin> initializePlugin);
	}

	public interface IPluginConfigurator : IGenericPluginConfigurator<object, IPluginConfigurator>
	{
		IPluginConfigurator<TPlugin> TypedConfigurator<TPlugin>();
	}

	public interface IPluginConfigurator<TPlugin> : IGenericPluginConfigurator<TPlugin, IPluginConfigurator<TPlugin>>
	{
	}
}