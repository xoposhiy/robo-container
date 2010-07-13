using System;

namespace RoboContainer.Core
{
	/// <para>Интерфейс является частью Fluent API для конфигурирования контейнера. 
	/// Вот пример использования этой части API:
	/// <code>
	///	new Container(
	///		c => c.Logging.SomeMemberOfThisInterface
	///		)
	/// </code>
	/// </para>
	public interface ILoggingConfigurator
	{
		/// <summary>
		/// Отключение логгирования
		/// </summary>
		ILoggingConfigurator Disable();

		/// <summary>
		/// Отключение логгирования, при условии <paramref name="whenDisable"/>
		/// </summary>
		ILoggingConfigurator DisableWhen(Func<bool> whenDisable);

		/// <summary>
		/// Замена стандартной реализации логгирования на пользовательскую реализацию.
		/// </summary>
		ILoggingConfigurator UseLogger(IConstructionLogger logger);
	}
}