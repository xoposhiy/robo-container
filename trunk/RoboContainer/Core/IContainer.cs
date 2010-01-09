using System;
using System.Collections.Generic;

namespace RoboContainer.Core
{
	public interface IContainer : IDisposable
	{
		TPlugin TryGet<TPlugin>(params ContractRequirement[] requiredContracts);
		TPlugin Get<TPlugin>(params ContractRequirement[] requiredContracts);
		IEnumerable<TPlugin> GetAll<TPlugin>(params ContractRequirement[] requiredContracts);
		object TryGet(Type pluginType, params ContractRequirement[] requiredContracts);
		object Get(Type pluginType, params ContractRequirement[] requiredContracts);
		IEnumerable<object> GetAll(Type pluginType, params ContractRequirement[] requiredContracts);
		IEnumerable<Type> GetPluggableTypesFor<TPlugin>(params ContractRequirement[] requiredContracts);
		IEnumerable<Type> GetPluggableTypesFor(Type pluginType, params ContractRequirement[] requiredContracts);
		IContainer With(Action<IContainerConfigurator> configure);
		string LastConstructionLog { get; }
	}
}