namespace RoboContainer.Core
{
	/// <summary>
	/// Абстрактный класс, представляющий собой определение контракта.
	/// Имеет неявный оператор приведения типов, конвертирующий строку в экземпляр <see cref="NamedContractDeclaration"/>.
	/// </summary>
	public abstract class ContractDeclaration
	{
		static ContractDeclaration()
		{
			Default = new DefaultContractDeclaration();
		}

		public static ContractDeclaration Default { get; private set; }

		/// <summary>
		/// В наследниках, метод должен определять, подходит ли данный контракт под требование <paramref name="requirement"/>.
		/// </summary>
		public abstract bool Satisfy(ContractRequirement requirement);

		public static implicit operator ContractDeclaration(string contractName)
		{
			return new NamedContractDeclaration(contractName);
		}

		private class DefaultContractDeclaration : ContractDeclaration
		{
			public override bool Satisfy(ContractRequirement requirement)
			{
				return requirement == ContractRequirement.Default;
			}

			public override string ToString()
			{
				return "DEFAULT";
			}
		}
	}
}