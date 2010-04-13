using System;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Infection
{
	/// <summary>
	/// <para>
	/// Если параметр конструктора помечен этим атрибутом, то контейнер будет инжектировать только те реализации,
	/// у которых есть строковый контракт, совпадающий с именем параметра конструктора.
	/// </para>
	/// <para>
	/// Список поддерживаемых контрактов может быть расширен с помощью других способов конфигурирования.
	/// </para>
	/// </summary>
	/// <seealso cref="IDependencyConfigurator.RequireContracts(ContractRequirement[])"/>
	/// <seealso cref="IGenericPluginConfigurator{TPlugin,TSelf}.RequireContracts(ContractRequirement[])"/>
	/// <seealso cref="RequireContractAttribute"/>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
	public class NameIsContractAttribute : Attribute
	{
	}
}