using System;
using System.ComponentModel;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	/// <summary>
	/// Интерфейс для конфигурирования работы контейнера, когда запрашивается заданный тип сервиса.
	/// <para>Интерфейс является частью Fluent API для конфигурирования контейнера. Вот пример использования этой части API:
	/// <code>
	///	new Container(
	///		c => c.ForPlugin(pluginType)
	///			.SomeMemberOfThisInterface
	///		)
	/// </code>
	/// </para>
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IGenericPluginConfigurator<TPlugin, TSelf>
	{
		/// <summary>
		/// Использовать в качестве реализации тип <typeparamref name="TPluggable"/>, считая, что он поддерживает указанный список контрактов.
		/// Если список контрактов пуст, считается, что тип поддерживает один контракт <see cref="ContractDeclaration.Default"/>.
		/// При повторном вызове список pluggable пополняется новым значением.
		/// </summary>
		[Additive]
		TSelf UsePluggable<TPluggable>(params ContractDeclaration[] declaredContracts) where TPluggable : TPlugin;

		/// <summary>
		/// Использовать в качестве реализации тип <paramref name="pluggableType"/>, считая, что он поддерживает указанный список контрактов.
		/// Если список контрактов пуст, считается, что тип поддерживает один контракт <see cref="ContractDeclaration.Default"/>.
		/// При повторном вызове список pluggable пополняется новым значением.
		/// </summary>
		[Additive]
		TSelf UsePluggable(Type pluggableType, params ContractDeclaration[] declaredContracts);

		/// <summary>
		/// Использовать в качестве реализации объект <paramref name="instance"/>, считая, что он поддерживает указанный список контрактов.
		/// Если список контрактов пуст, считается, что тип поддерживает один контракт <see cref="ContractDeclaration.Default"/>.
		/// При повторном вызове список pluggable пополняется новым значением.
		/// </summary>
		[Additive]
		TSelf UseInstance(TPlugin instance, params ContractDeclaration[] declaredContracts);

		/// <summary>
		/// Использовать в качестве реализации результат работы делегата <paramref name="createPluggable"/>, считая, что он поддерживает указанный список контрактов.
		/// Если список контрактов пуст, считается, что тип поддерживает один контракт <see cref="ContractDeclaration.Default"/>.
		/// При повторном вызове список pluggable пополняется новым значением.
		/// </summary>
		[Additive]
		TSelf UseInstanceCreatedBy(CreatePluggableDelegate<TPlugin> createPluggable, params ContractDeclaration[] declaredContracts);

		/// <summary>
		/// Для данного сервиса использовать только те реализации, список декларируемых контрактов которых удовлетворяет заданному списку требований.
		/// При повторном вызове список требуемых контрактов пополняется новыми значениями.
		/// </summary>
		[Additive]
		TSelf RequireContracts(params ContractRequirement[] requiredContracts);

		/// <summary>
		/// При работе автоматики не использовать реализацию <typeparam name="TPluggable"/>.
		/// При повторном вызове список игнорируемых реализаций пополняется новым значением.
		/// </summary>
		[Additive]
		TSelf DontUse<TPluggable>() where TPluggable : TPlugin;

		/// <summary>
		/// Не использовать реализации <paramref name="pluggableTypes"/>
		/// При повторном вызове список игнорируемых реализаций пополняется новыми значениями.
		/// </summary>
		[Additive]
		TSelf DontUse(params Type[] pluggableTypes);

		/// <summary>
		/// Использовать ли, кроме явно указанных способов получения реализаций, ещё и все реализации, найденные автоматикой.
		/// Если этот метод не вызван, то автоматика используется тогда и только тогда, когда явно не указан ни один способ получения реализаций.
		/// </summary>
		[Overridable]
		TSelf UsePluggablesAutosearch(bool useAutosearch);

		/// <summary>
		/// Для всех реализаций, созданных для данного сервиса применить указанную политику повторного использования.
		/// При повторном вызове старая политика повторного используется заменяется на новую.
		/// По умолчанию полтитика <see cref="Reuse.InSameContainer"/>.
		/// </summary>
		[Overridable]
		TSelf ReusePluggable(ReusePolicy reusePolicy);

		/// <summary>
		/// Для всех реализаций, созданных для данного сервиса применить указанную политику повторного использования.
		/// При повторном вызове старая политика повторного используется заменяется на новую.
		/// По умолчанию полтитика <see cref="Reuse.InSameContainer"/>.
		/// </summary>
		[Overridable]
		TSelf ReusePluggable(IReusePolicy reusePolicy);

		/// <summary>
		/// Вызвать указанный делегат сразу после создания каждой реализации данного сервиса.
		/// При повторном вызове старый инициализатор заменяется на новый.
		/// </summary>
		[Overridable]
		TSelf SetInitializer(InitializePluggableDelegate<TPlugin> initializePluggable);

		/// <summary>
		/// Вызвать указанный делегат сразу после создания каждой реализации данного сервиса.
		/// При повторном вызове старый инициализатор заменяется на новый.
		/// </summary>
		[Overridable]
		TSelf SetInitializer(Action<TPlugin> initializePlugin);
	}
}