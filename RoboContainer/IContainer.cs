﻿using System;
using System.Collections.Generic;

namespace RoboContainer
{
	public interface IContainer
	{
		TPlugin Get<TPlugin>(params ContractRequirement[] requiredContracts);
		IEnumerable<TPlugin> GetAll<TPlugin>(params ContractRequirement[] requiredContracts);
		object Get(Type pluginType, params ContractRequirement[] requiredContracts);
		IEnumerable<object> GetAll(Type pluginType, params ContractRequirement[] requiredContracts);
		IEnumerable<Type> GetPluggableTypesFor<TPlugin>(params ContractRequirement[] requiredContracts);
		IEnumerable<Type> GetPluggableTypesFor(Type pluginType, params ContractRequirement[] requiredContracts);
		IContainer With(Action<IContainerConfigurator> configure);
	}
}