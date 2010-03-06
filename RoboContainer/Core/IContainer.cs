using System;
using System.Collections.Generic;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public interface IContainer : IDisposable
	{
		string LastConstructionLog { get; }

		/// <summary>
		/// Создает или возвращает созданный ранее экземпляр типа <typeparamref name="TPlugin"/>, 
		/// удовлетворяющий каждому контракту из списка <paramref name="requiredContracts"/>. 
		/// Если ни одного контракта не указано, то считается, что нужна реализация с контрактом "DEFAULT".
		/// </summary>
		/// <returns>null, если не найдено ни одной подходящей реализации.</returns>
		/// <exception cref="ContainerException">
		/// Нашлось более одной подходящей реализации.
		/// Случилась ошибка при создании реализации.
		/// </exception>
		TPlugin TryGet<TPlugin>(params ContractRequirement[] requiredContracts);

		/// <summary>
		/// Нетипизированная версия метода <see cref="TryGet{TPlugin}"/>.
		/// </summary>
		object TryGet(Type pluginType, params ContractRequirement[] requiredContracts);

		/// <summary>
		/// Создает или возвращает созданный ранее экземпляр типа <typeparamref name="TPlugin"/>,
		/// удовлетворяющий каждому контракту из списка <paramref name="requiredContracts"/>.
		/// Если ни одного контракта не указано, то считается, что нужна реализация с контрактом "DEFAULT".
		/// </summary>
		/// <exception cref="ContainerException">
		/// Не нашлось ни одной подходящей реализации.
		/// Нашлось более одной подходящей реализации.
		/// Случилась ошибка при создании реализации.
		/// </exception>
		TPlugin Get<TPlugin>(params ContractRequirement[] requiredContracts);

		/// <summary>
		/// Нетипизированная версия метода <see cref="Get{TPlugin}"/>.
		/// </summary>
		object Get(Type pluginType, params ContractRequirement[] requiredContracts);

		/// <summary>
		/// Создает или возвращает созданные ранее экземпляры типа <typeparamref name="TPlugin"/>, 
		/// удовлетворяющие каждому контракту из списка <paramref name="requiredContracts"/>.
		/// Возвращаются все возможные реализации.
		/// Если ни одного контракта не указано, то считается, что нужна реализация с контрактом "DEFAULT".
		/// </summary>
		/// <exception cref="ContainerException">
		/// Случилась ошибка при создании одной из реализаций.
		/// </exception>
		IEnumerable<TPlugin> GetAll<TPlugin>(params ContractRequirement[] requiredContracts);

		/// <summary>
		/// Нетипизированная версия метода <see cref="GetAll{TPlugin}"/>.
		/// </summary>
		IEnumerable<object> GetAll(Type pluginType, params ContractRequirement[] requiredContracts);

		/// <summary>
		/// Возвращает все типы, которые могут использоваться в качестве реализаций сервиса <typeparamref name="TPlugin"/>,
		/// реализующих список контрактов <paramref name="requiredContracts"/>.
		/// <para>Для некоторых способов получить реализацию сервиса, контейнер не может определить тип.
		/// Например, так бывает для реализаций, создаваемых делегатами.
		/// В этом случае, тип этой реализации не присутствует в результате.
		/// И наоборот, некоторые подходящие типы, возвращаемые этим методом, возможно, не получится инстанциировать,
		/// и аналогичный вызов GetAll не будет содержать соответствующих объектов.
		/// </para>
		/// <para>
		/// Этот метод позволяет получить список плагинов, без собственно создания объектов этих плагинов. 
		/// Это может быть полезно, если по типам плагинов нужно всего лишь узнать некоторую метаинформацию.
		/// </para>
		/// </summary>
		IEnumerable<Type> GetPluggableTypesFor<TPlugin>(params ContractRequirement[] requiredContracts);

		/// <summary>
		/// Нетипизированная версия метода <see cref="GetPluggableTypesFor{TPlugin}"/>.
		/// </summary>
		IEnumerable<Type> GetPluggableTypesFor(Type pluginType, params ContractRequirement[] requiredContracts);

		/// <summary>
		/// Создает дочерний контейнер, который можно доконфигурировать. 
		/// Конфигурирование дочернего контейнера никак не влияет на контейнер-родитель.
		/// </summary>
		IContainer With(Action<IContainerConfigurator> configure);
	}
}