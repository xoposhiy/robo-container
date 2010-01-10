using System;
using RoboContainer.Impl;

namespace RoboContainer.Infection
{
	/// <summary>
	/// Если класс помечен этим атрибутом, то считается, что он поддерживает все перечисленные контракты.
	/// Список поддерживаемых контрактов может быть расширен при явном динамическом конфигурировании.
	/// <seealso cref="IGenericPluggableConfigurator{TPluggable,TSelf}.DeclareContracts(RoboContainer.Core.ContractDeclaration[])"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DeclareContractAttribute : Attribute
	{
		public DeclareContractAttribute(params string[] contracts)
		{
			Contracts = contracts;
		}

		public string[] Contracts { get; private set; }
	}
}