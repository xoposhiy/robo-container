using System;
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
	public interface IGenericPluginConfigurator<TPlugin, TSelf>
	{
		/// <summary>
		/// Использовать в качестве реализации тип <typeparamref name="TPluggable"/>, считая, что он поддерживает указанный список контрактов.
		/// Если список контрактов пуст, считается, что тип поддерживает один контракт <see cref="ContractDeclaration.Default"/>.
		/// При повторном вызове список pluggable пополняется новым значением.
		/// </summary>
		TSelf UsePluggable<TPluggable>(params ContractDeclaration[] declaredContracts) where TPluggable : TPlugin;

		/// <summary>
		/// Использовать в качестве реализации тип <paramref name="pluggableType"/>, считая, что он поддерживает указанный список контрактов.
		/// Если список контрактов пуст, считается, что тип поддерживает один контракт <see cref="ContractDeclaration.Default"/>.
		/// При повторном вызове список pluggable пополняется новым значением.
		/// </summary>
		TSelf UsePluggable(Type pluggableType, params ContractDeclaration[] declaredContracts);

		/// <summary>
		/// Использовать в качестве реализации объект <paramref name="instance"/>, считая, что он поддерживает указанный список контрактов.
		/// Если список контрактов пуст, считается, что тип поддерживает один контракт <see cref="ContractDeclaration.Default"/>.
		/// При повторном вызове список pluggable пополняется новым значением.
		/// </summary>
		TSelf UseInstance(TPlugin instance, params ContractDeclaration[] declaredContracts);

		/// <summary>
		/// Использовать в качестве реализации результат работы делегата <paramref name="createPluggable"/>, считая, что он поддерживает указанный список контрактов.
		/// Если список контрактов пуст, считается, что тип поддерживает один контракт <see cref="ContractDeclaration.Default"/>.
		/// При повторном вызове список pluggable пополняется новым значением.
		/// </summary>
		TSelf UseInstanceCreatedBy(CreatePluggableDelegate<TPlugin> createPluggable, params ContractDeclaration[] declaredContracts);

		/// <summary>
		/// Использовать, кроме явно указанных способов получения реализаций, ещё и все реализации, найденные автоматикой.
		/// Если явно не указан ни один способ получения реализаций, то используется автоматика, даже если этот метод не вызван.
		/// </summary>
		TSelf UseAutoFoundPluggables();

		/// <summary>
		/// Не использовать автоматический поиск реализаций. Использовать только явно указанные способы получения реализаций.
		/// Если явно указан хотя бы один способ получения реализаций, то автоматика не используется, даже если этот метод не вызван.
		/// </summary>
		TSelf DontUseAutoFoundPluggables();

		/// <summary>
		/// При работе автоматики не использовать реализацию <typeparam name="TPluggable"/>.
		/// При повторном вызове список игнорируемых реализаций пополняется новым значением.
		/// </summary>
		TSelf DontUse<TPluggable>() where TPluggable : TPlugin;

		/// <summary>
		/// Не использовать реализации <paramref name="pluggableTypes"/>
		/// При повторном вызове список игнорируемых реализаций пополняется новыми значениями.
		/// </summary>
		TSelf DontUse(params Type[] pluggableTypes);

		/// <summary>
		/// Для всех реализаций, созданных для данного сервиса применить указанную политику повторного использования.
		/// При повторном вызове старая политика повторного используется заменяется на новую.
		/// </summary>
		TSelf ReusePluggable(ReusePolicy reusePolicy);

		/// <summary>
		/// Для всех реализаций, созданных для данного сервиса применить указанную политику повторного использования.
		/// При повторном вызове старая политика повторного используется заменяется на новую.
		/// </summary>
		TSelf ReusePluggable(IReusePolicy reusePolicy);

		/// <summary>
		/// Для данного сервиса использовать только те реализации, список декларируемых контрактов которых удовлетворяет заданному списку требований.
		/// При повторном вызове список требуемых контрактов пополняется новыми значениями.
		/// </summary>
		TSelf RequireContracts(params ContractRequirement[] requiredContracts);

		/// <summary>
		/// Вызвать указанный делегат сразу после создания каждой реализации данного сервиса.
		/// При повторном вызове старый инициализатор заменяется на новый.
		/// </summary>
		TSelf SetInitializer(InitializePluggableDelegate<TPlugin> initializePluggable);

		/// <summary>
		/// Вызвать указанный делегат сразу после создания каждой реализации данного сервиса.
		/// При повторном вызове старый инициализатор заменяется на новый.
		/// </summary>
		TSelf SetInitializer(Action<TPlugin> initializePlugin);
	}
}