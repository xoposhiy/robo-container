using System;
using System.Collections.Generic;
using RoboContainer.Core;
using RoboContainer.Impl;
using System.Linq;

namespace RoboContainer.Infection
{
	/// <summary>
	/// Если класс помечен этим атрибутом, то считается, что он поддерживает все перечисленные контракты.
	/// Список поддерживаемых контрактов может быть расширен при явном динамическом конфигурировании.
	/// <seealso cref="IGenericPluggableConfigurator{TPluggable,TSelf}.DeclareContracts(RoboContainer.Core.ContractDeclaration[])"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
	public class DeclareContractAttribute : Attribute
	{
		public DeclareContractAttribute(params string[] contracts)
			: this(contracts.Select(c => (ContractDeclaration)c))
		{
		}
		public DeclareContractAttribute(params Type[] contracts)
			: this(contracts.Select(c => (ContractDeclaration)c))
		{
		}

		public DeclareContractAttribute(IEnumerable<ContractDeclaration> contracts)
		{
			Contracts = contracts.ToArray();
		}

		public ContractDeclaration[] Contracts { get; private set; }
	}
}