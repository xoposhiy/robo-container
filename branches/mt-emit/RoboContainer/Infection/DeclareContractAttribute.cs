using System;
using RoboContainer.Impl;
using System.Linq;

namespace RoboContainer.Infection
{
	/// <summary>
	/// Если класс помечен этим атрибутом, то считается, что он поддерживает все перечисленные контракты.
	/// Список поддерживаемых контрактов может быть расширен при явном динамическом конфигурировании.
	/// <seealso cref="IGenericPluggableConfigurator{TPluggable,TSelf}.DeclareContracts(string[])"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
	public class DeclareContractAttribute : Attribute
	{
		public DeclareContractAttribute(params string[] contracts)
		{
			Contracts = contracts;
		}

		public DeclareContractAttribute(params Type[] contracts)
			: this(contracts.Select(c => c.Name).ToArray())
		{
		}

		public string[] Contracts { get; private set; }
	}
}