using System;

namespace RoboContainer.Core
{
	/// <summary>
	/// Интерфейс подсистемы логгирования контейнера.
	/// </summary>
	public interface IConstructionLogger
	{
		/// <summary>
		/// Метод вызывается в начале каждой сессии логгирования. Сессии логгирования могут быть вложенными. 
		/// В момент завершения сессии логгирования вызывается метод <see cref="IDisposable.Dispose"/> у объекта, 
		/// возвращенного этим методом.
		/// </summary>
		/// <param name="pluginType">тип запрашиваемого у контейнера объекта</param>
		IDisposable StartConstruction(Type pluginType);

		/// <summary>
		/// Вызывается контейнером сразу после того, как экземпляр <paramref name="pluggableType"/> был создан.
		/// </summary>
		void Constructed(Type pluggableType);

		/// <summary>
		/// Вызывается контейнером сразу после того, как был вызван код инициализации экземпляра <paramref name="pluggableType"/>.
		/// </summary>
		void Initialized(Type pluggableType);

		/// <summary>
		/// Вызывается контейнером, если в процессе конструирования объекта <paramref name="pluggableType"/> возникла ошибка.
		/// </summary>
		void ConstructionFailed(Type pluggableType);

		/// <summary>
		/// Вызывается контейнером, если при запросе экземпляра <paramref name="pluggableType"/> контейнер возвращает объект, созданный ранее.
		/// </summary>
		void Reused(Type pluggableType);

		/// <summary>
		/// Вызывается контейнером, когда реализация <paramref name="pluggableType"/> была отвергнута контейнером по некоторой причине. 
		/// Например, по причине того, что у данной реализации список декларируемых контрактов не удовлетворяет запрошенный список требований контрактов.
		/// </summary>
		void Declined(Type pluggableType, string reason);

		string ToString();
	}
}