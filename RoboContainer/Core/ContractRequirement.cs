using System;
using System.Linq;

namespace RoboContainer.Core
{
	/// <summary>
	/// Абстрактный класс, представляющий собой определение требования контракта.
	/// Имеет неявный оператор приведения типов, конвертирующий строку в экземпляр <see cref="SimpleContractRequirement{String}"/>.
	/// </summary>
	public abstract class ContractRequirement
	{
		static ContractRequirement()
		{
			Default = "DEFAULT";
		}

		public static ContractRequirement Default { get; private set; }

		public static implicit operator ContractRequirement(string value)
		{
			return new SimpleContractRequirement<string>(value);
		}

		public static implicit operator ContractRequirement(Type type)
		{
			return type.FullName;
		}
	}

	public static class InjectionContracts
	{
		public static bool IsContractAttribute(object attribute)
		{
			return IsContractAttribute(attribute.GetType());
		}

		public static bool IsContractAttribute(Type attributeType)
		{
			return attributeType.GetCustomAttributes(true).Any(a => MeansRequiredContractAttribute(a.GetType()));
		}

		public static bool MeansRequiredContractAttribute(Type attributeType)
		{
			var typename = attributeType.Name;
			return typename == "MeansInjectionContractAttribute" || typename == "MeansInjectionContract";
		}
	}
}