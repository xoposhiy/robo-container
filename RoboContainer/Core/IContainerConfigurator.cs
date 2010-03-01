using System;
using System.Collections.Generic;
using System.Reflection;

namespace RoboContainer.Core
{
	public delegate void ScannerDelegate(IContainerConfigurator configurator, Type type);

	/// <summary>
	/// Интерфейс для конфигурирования контейнера.
	/// <para>Интерфейс является частью Fluent API для конфигурирования контейнера. Вот пример использования этой части API:
	/// <code>
	///	new Container(
	///		c => c.SomeMemberOfThisInterface
	///		)
	/// </code>
	/// </para>
	/// </summary>
	public interface IContainerConfigurator
	{
		/// <summary>
		/// Конфигурирование подсистемы логгирования.
		/// </summary>
		ILoggingConfigurator Logging { get; }

		/// <summary>
		/// Рассматривать все публичные типы из указанного списка сборок.
		/// </summary>
		void ScanAssemblies(params Assembly[] assembliesToScan);

		/// <summary>
		/// Рассматривать все публичные типы из указанного списка сборок.
		/// </summary>
		void ScanAssemblies(IEnumerable<Assembly> assembliesToScan);

		/// <summary>
		/// Рассматривать все публичные типы из той сборки, в которой находится вызвавший код.
		/// </summary>
		void ScanCallingAssembly();

		/// <summary>
		/// Рассматривать все публичные типы из всех загруженных в данный момент сборок.
		/// Может быть довольно медлено, поскольку включает все сборки .NET Framework.
		/// </summary>
		void ScanLoadedAssemblies();

		/// <summary>
		/// Рассматривать все публичные типы из всех загруженных в данный момент сборок, 
		/// удовлетворяющих условию <paramref name="shouldScan"/>.
		/// </summary>
		void ScanLoadedAssemblies(Func<Assembly, bool> shouldScan);

		/// <summary>
		/// Перебирает все публичные типы из всех загруженных в данный момент сборок, 
		/// и передает каждый из них в делегат <paramref name="scanner"/>.
		/// </summary>
		void ScanTypesWith(ScannerDelegate scanner);

		/// <summary>
		/// Конфигурирование создания экземпляра типа <paramref name="pluggableType"/>.
		/// </summary>
		IPluggableConfigurator ForPluggable(Type pluggableType);

		/// <summary>
		/// Конфигурирование создания экземпляра типа <typeparamref name="TPluggable"/>.
		/// </summary>
		IPluggableConfigurator<TPluggable> ForPluggable<TPluggable>();

		/// <summary>
		/// Конфигурирование создания реализаций сервиса <typeparamref name="TPlugin"/>.
		/// </summary>
		IPluginConfigurator<TPlugin> ForPlugin<TPlugin>();

		/// <summary>
		/// Конфигурирование создания реализаций сервиса <paramref name="pluginType"/>.
		/// </summary>
		IPluginConfigurator ForPlugin(Type pluginType);
	}

	public static class ContainerConfigurationExtensions
	{
		public static void Bind<TPlugin>(this IContainerConfigurator configurator, TPlugin value)
		{
			configurator.ForPlugin<TPlugin>().UseInstance(value);
		}

		public static void Bind<TPlugin, TPluggable>(this IContainerConfigurator configurator) where TPluggable : TPlugin
		{
			configurator.ForPlugin<TPlugin>().UsePluggable<TPluggable>();
		}

		public static void Bind<TPlugin, TPluggable>(this IContainerConfigurator configurator, ReusePolicy reusePolicy) where TPluggable : TPlugin
		{
			configurator.ForPlugin<TPlugin>().UsePluggable<TPluggable>().ReusePluggable(reusePolicy);
		}
	}

}