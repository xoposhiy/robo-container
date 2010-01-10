using System;

namespace RoboContainer.Core
{
	/// <summary>
	/// Интерфейс конфигурирования одной из зависимостей у некоторой реализации. 
	/// <para>Интерфейс является частью Fluent API для конфигурирования контейнера. Вот пример использования этой части API:
	/// <code>
	///	new Container(
	///		c => c.ForPluggable(pluggableType)
	///			.Dependency(dependencyName)
	///			.SomeMemberOfThisInterface
	///		)
	/// </code>
	/// </para>
	/// </summary>
	public interface IDependencyConfigurator
	{
		/// <summary>
		/// Использовать в качестве значения для этой зависимости только те реализации, 
		/// которые удовлетворяют указанному списку требований <paramref name="requiredContracts"/>.
		/// </summary>
		IDependencyConfigurator RequireContracts(params ContractRequirement[] requiredContracts);

		/// <summary>
		/// Использовать в качестве значения для этой зависимости объект <paramref name="o"/>
		/// </summary>
		IDependencyConfigurator UseValue(object o);

		/// <summary>
		/// Использовать в качестве значения для этой зависимости реализацию типа <paramref name="pluggableType"/>
		/// </summary>
		IDependencyConfigurator UsePluggable(Type pluggableType);

		/// <summary>
		/// Использовать в качестве значения для этой зависимости реализацию типа <typeparamref name="TPluggable"/>
		/// </summary>
		IDependencyConfigurator UsePluggable<TPluggable>();

		//TODO Поддержка UsePluggable через атрибуты.
	}
}