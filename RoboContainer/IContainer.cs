using System;
using System.Collections.Generic;
using RoboContainer.Impl;

namespace RoboContainer
{
	public interface IContainer
	{
		TPlugin Get<TPlugin>(params IContractRequirement[] requiredContracts);
		IEnumerable<TPlugin> GetAll<TPlugin>(params IContractRequirement[] requiredContracts);
		object Get(Type pluginType, params IContractRequirement[] requiredContracts);
		IEnumerable<object> GetAll(Type pluginType, params IContractRequirement[] requiredContracts);
		IEnumerable<Type> GetPluggableTypesFor<TPlugin>(params IContractRequirement[] requiredContracts);
		IEnumerable<Type> GetPluggableTypesFor(Type pluginType, params IContractRequirement[] requiredContracts);
		IContainer With(Action<IContainerConfigurator> configure);
	}
}