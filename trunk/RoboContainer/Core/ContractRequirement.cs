using System;
using System.Linq;

namespace RoboContainer.Core
{
	/// <summary>
	/// Абстрактный класс, представляющий собой определение требования контракта.
	/// Имеет неявный оператор приведения типов, конвертирующий строку в экземпляр <see cref="SimpleContractRequirement{String}"/>.
	/// <para>Наследники должны обязательно переопределять методы Equals и GetHashCode. 
	/// Иначе не сможет корректно работать детектор циклических зависимостей.</para>
	/// </summary>
	public abstract class ContractRequirement
	{
		static ContractRequirement()
		{
			Default = "DEFAULT";
			Anyone = new AnyoneRequirement();
		}

		private class AnyoneRequirement : ContractRequirement
		{
			public override bool Satisfy(ContractDeclaration declaration)
			{
				return true;
			}
		}

		public static ContractRequirement Default { get; private set; }

		/// <summary>
		/// В наследниках, метод должен определять, подходит ли данный контракт под требование.
		/// </summary>
		public abstract bool Satisfy(ContractDeclaration declaration);

		public static ContractRequirement Anyone
		{
			get; private set;
		}

		public static implicit operator ContractRequirement(string value)
		{
			return new StringContractRequirement(value);
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