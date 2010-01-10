using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	/// <summary>
	/// Интерфейс конфигурирования процесса создания экземпляров некоторого типа.
	/// <para>Интерфейс является частью Fluent API для конфигурирования контейнера. Вот пример использования этой части API:
	/// <code>
	///	new Container(
	///		c => c.ForPluggable(pluggableType)
	///			.SomeMemberOfThisInterface
	///		)
	/// </code>
	/// </para>
	/// </summary>
	public interface IGenericPluggableConfigurator<TPluggable, TSelf>
	{
		/// <summary>
		/// Определяет контракты, которые предоставляются данной реализацией.
		/// </summary>
		TSelf DeclareContracts(params ContractDeclaration[] contractsDeclaration);

		/// <summary>
		/// Определяет политику повторного использования созданных экземпляров данного типа.
		/// </summary>
		TSelf ReuseIt(ReusePolicy reusePolicy);

		/// <summary>
		/// Альтернативный, более гибкий способ определения политики повторного использования созданных экземпляров данного типа.
		/// </summary>
		TSelf ReuseIt<TReuse>() where TReuse : IReuse, new();

		/// <summary>
		/// Игнорирование данного типа при автоматическом поиске реализаций контейнером.
		/// </summary>
		TSelf DontUseIt();

		/// <summary>
		/// Явный выбор конструктора, который должен использовать контейнер для создания экземпляров данного типа.
		/// </summary>
		TSelf UseConstructor(params Type[] argsTypes);

		/// <summary>
		/// Указание делегата, который будет выполнен сразу после создания каждого экземпляра данного типа.
		/// </summary>
		TSelf SetInitializer(InitializePluggableDelegate<TPluggable> initializePluggable);

		/// <summary>
		/// Указание делегата, который будет выполнен сразу после создания каждого экземпляра данного типа.
		/// </summary>
		TSelf SetInitializer(Action<TPluggable> initializePluggable);

		/// <summary>
		/// Конфигурирование зависимости с именем <paramref name="dependencyName"/>.
		/// </summary>
		IDependencyConfigurator Dependency(string dependencyName);

		//TODO Поддержка ExportedPart
	}
}