﻿using System;
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
		/// При повторном вызове список контрактов пополняется новыми значениями.
		/// Контракты родительского и дочернего контейнера объединяются.
		/// </summary>
		[Additive]
		TSelf DeclareContracts(params ContractDeclaration[] contractsDeclaration);

		/// <summary>
		/// Определяет политику повторного использования созданных экземпляров данного типа.
		/// При повторном вызове старая политика заменяется новой.
		/// Политика дочернего контейнера перекрывает политику родительского.
		/// </summary>
		[Overridable]
		TSelf ReuseIt(ReusePolicy reusePolicy);

		/// <summary>
		/// Альтернативный, более гибкий способ определения политики повторного использования созданных экземпляров данного типа.
		/// При повторном вызове старая политика заменяется новой.
		/// Политика дочернего контейнера перекрывает политику родительского.
		/// </summary>
		[Overridable]
		TSelf ReuseIt(IReusePolicy reusePolicy);

		/// <summary>
		/// Игнорирование данного типа при автоматическом поиске реализаций контейнером. 
		/// Pluggable после вызова этого метода все ещё может быть использован в качестве реализации, 
		/// если его указали при явном конфигурировании Plugin-а.
		/// В дочернем контейнере тип считается заигнорированным, если он заигнорирован в родительском или дочернем контейнере.
		/// </summary>
		[Additive]
		TSelf DontUseIt();

		/// <summary>
		/// Явный выбор конструктора, который должен использовать контейнер для создания экземпляров данного типа.
		/// При повторном вызове старое значение заменяется новым.
		/// Конструктор, выбранный в дочернем контейнере перекрывает конструктор, выбранный в родительском контейнере.
		/// </summary>
		[Overridable]
		TSelf UseConstructor(params Type[] argsTypes);

		/// <summary>
		/// Указание делегата, который будет выполнен сразу после создания каждого экземпляра данного типа.
		/// При повторном вызове старое значение заменяется новым.
		/// </summary>
		[Overridable]
		TSelf SetInitializer(InitializePluggableDelegate<TPluggable> initializePluggable);

		/// <summary>
		/// Указание делегата, который будет выполнен сразу после создания каждого экземпляра данного типа.
		/// При повторном вызове старое значение заменяется новым.
		/// </summary>
		// TODO возможно следует сделать этот метод аддитивным (чтобы можно было задавать несколько инициализаторов). 
		// Тогда будет человечнее и интуитивнее его поведение при доконфигурировании контейнера через With.
		[Overridable]
		TSelf SetInitializer(Action<TPluggable> initializePluggable);

		/// <summary>
		/// Конфигурирование зависимости с именем <paramref name="dependencyName"/>.
		/// При повторном вызове возвращается конфигуратор, с помощью которого можно доконфигурировать зависимость.
		/// </summary>
		IDependencyConfigurator Dependency(string dependencyName);

		//TODO Поддержка ProvidePart
		//TSelf Exports<TPart>(Func<TPluggable, TPart> getExportedPart, params ContractDeclaration[] contractDeclarations);
	}

	public class OverridableAttribute : Attribute
	{
	}

	public class AdditiveAttribute : Attribute
	{
	}
}