using System;
using System.Collections.Generic;
using RoboContainer.Core;
using RoboContainer.Impl;
using System.Linq;

namespace RoboContainer.Infection
{
	/// <summary>
	/// <para>
	/// Если параметр конструктора помечен этим атрибутом, то контейнер будет инжектировать только те реализации,
	/// список поддерживаемых контрактов которого, удовлетворяет все указанные в этом атрибуте требования.
	/// </para>
	/// <para>
	/// Если этим атрибутом помечен интерфейс или абстрактный класс, то при запросе такого сервиса, контейнер будет искать только те реализации,
	/// список поддерживаемых контрактов которого, удовлетворяет все указанные в этом атрибуте требования.
	/// </para>
	/// <para>
	/// Список поддерживаемых контрактов может быть расширен при явном динамическом конфигурировании.
	/// </para>
	/// </summary>
	/// <seealso cref="IDependencyConfigurator.RequireContracts(ContractRequirement[])"/>
	/// <seealso cref="IGenericPluginConfigurator{TPlugin,TSelf}.RequireContracts(ContractRequirement[])"/>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
	public class RequireContractAttribute : Attribute
	{
		public RequireContractAttribute(params string[] contracts)
			: this(contracts.Select(c => (ContractRequirement)c))
		{
		}

		public RequireContractAttribute(params Type[] contracts)
			: this(contracts.Select(c => (ContractRequirement)c))
		{
		}

		public RequireContractAttribute(IEnumerable<ContractRequirement> contracts)
		{
			Contracts = contracts.ToArray();
		}

		public ContractRequirement[] Contracts { get; private set; }
	}
}