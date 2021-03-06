﻿using System;
using System.ComponentModel;
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
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IGenericPluggableConfigurator<TPluggable, TSelf>
	{
		/// <summary>
		/// Определяет контракты, которые предоставляются данной реализацией.
		/// При повторном вызове список контрактов пополняется новыми значениями.
		/// Контракты родительского и дочернего контейнера объединяются.
		/// </summary>
		[Additive]
		TSelf DeclareContracts(params string[] contractsDeclaration);

		/// <summary>
		/// Определяет политику повторного использования созданных экземпляров данного типа.
		/// При повторном вызове старая политика заменяется новой.
		/// Политика дочернего контейнера перекрывает политику родительского.
		/// По умолчанию полтитика <see cref="Reuse.InSameContainer"/>.
		/// </summary>
		[Overridable]
		TSelf ReuseIt(ReusePolicy reusePolicy);

		/// <summary>
		/// Альтернативный, более гибкий способ определения политики повторного использования созданных экземпляров данного типа.
		/// При повторном вызове старая политика заменяется новой.
		/// Политика дочернего контейнера перекрывает политику родительского.
		/// По умолчанию полтитика <see cref="Reuse.InSameContainer"/>.
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
		/// Явный выбор конструктора, который должен использоваться контейнером для создания экземпляров данного типа.
		/// При повторном вызове старое значение заменяется новым.
		/// </summary>
		[Overridable]
		TSelf UseConstructor(params Type[] argsTypes);

		/// <summary>
		/// Задает способ создания данного конкретного типа.
		/// При повторном вызове старое значение заменяется новым.
		/// </summary>
		[Overridable]
		TSelf CreateItBy(CreatePluggableDelegate<TPluggable> create);

		/// <summary>
		/// Задание конкретного экземпляра, который нужно использовать для типа <typeparamref name="TPluggable"/>.
		/// При повторном вызове старое значение заменяется новым.
		/// </summary>
		[Overridable]
		TSelf UseInstance(TPluggable instance);

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
		[Overridable]
		TSelf SetInitializer(Action<TPluggable> initializePluggable);

		/// <summary>
		/// Конфигурирование зависимости с именем <paramref name="dependencyName"/>.
		/// При повторном вызове возвращается конфигуратор, с помощью которого можно доконфигурировать зависимость.
		/// </summary>
		IDependencyConfigurator Dependency(string dependencyName);

		/// <summary>
		/// Конфигурирование зависимости с типом <typeparamref name="TDependencyType"/>.
		/// При повторном вызове возвращается конфигуратор, с помощью которого можно доконфигурировать зависимость.
		/// </summary>
		IDependencyConfigurator DependencyOfType<TDependencyType>();

		/// <summary>
		/// Конфигурирование зависимости с типом <paramref name="dependencyType"/>.
		/// При повторном вызове возвращается конфигуратор, с помощью которого можно доконфигурировать зависимость.
		/// </summary>
		IDependencyConfigurator DependencyOfType(Type dependencyType);
	}

	/// <summary>
	/// Повторный вызов метода, помеченного этим атрибутом, <b>полностью перекрывает</b> эффект от предыдущего вызова. То же относится и к вызовам из дочернего контейнера.
	/// </summary>
	public class OverridableAttribute : Attribute
	{
	}

	/// <summary>
	/// Повторный вызов метода, помеченного этим атрибутом, <b>дополняет</b> эффект от предыдущего вызова. То же относится и к вызовам из дочернего контейнера.
	/// </summary>
	public class AdditiveAttribute : Attribute
	{
	}
}