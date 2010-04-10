using System;

namespace RoboContainer.Core
{
	/// <summary>
	/// Абстрактный класс, представляющий собой определение контракта.
	/// Имеет неявный оператор приведения типов, конвертирующий строку в экземпляр <see cref="SimpleContractDeclaration{String}"/>.
	/// </summary>
	public abstract class ContractDeclaration
	{
		static ContractDeclaration()
		{
			Default = "DEFAULT";
		}

		public static ContractDeclaration Default { get; private set; }


		public static implicit operator ContractDeclaration(string contractName)
		{
			return new SimpleContractDeclaration<string>(contractName);
		}

		public static implicit operator ContractDeclaration(Type contractType)
		{
			return contractType.FullName;
		}
	}
}