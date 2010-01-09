﻿using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IGenericPluginConfigurator<TPlugin, TSelf>
	{
		TSelf UsePluggable<TPluggable>(params ContractDeclaration[] declaredContracts) where TPluggable : TPlugin;
		TSelf UsePluggable(Type pluggableType, params ContractDeclaration[] declaredContracts);
		TSelf UseInstance(TPlugin instance, params ContractDeclaration[] declaredContracts);
		TSelf UseInstanceCreatedBy(CreatePluggableDelegate<TPlugin> createPluggable);
		TSelf UseOtherPluggablesToo();
		TSelf DontUse<TPluggable>() where TPluggable : TPlugin;
		TSelf DontUse(params Type[] pluggableTypes);
		TSelf ReusePluggable(ReusePolicy reusePolicy);
		TSelf ReusePluggable<TReuse>() where TReuse : IReuse, new();
		TSelf RequireContracts(params ContractRequirement[] requiredContracts);
		TSelf SetInitializer(InitializePluggableDelegate<TPlugin> initializePluggable);
		TSelf SetInitializer(Action<TPlugin> initializePlugin);
	}
}