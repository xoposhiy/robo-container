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
		IExternalConfigurator ConfigureBy { get; }

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
		/// Рассматривать все публичные типы из загруженных в данный момент сборок, имена которых имеют общий префикс с именем вызывающей сборки.
		/// Например, если метод вызван из сборки MyCompany.Something, то ьудут рассмотрены все сборки с именем, начинающимся на "MyCompany.".
		/// </summary>
		void ScanLoadedCompanyAssemblies();

		/// <summary>
		/// Рассматривать все публичные типы из загруженных в данный момент сборок, имена которых начинаются на префикс <paramref name="companyPrefix"/>.
		/// </summary>
		void ScanLoadedAssembliesWithPrefix(string companyPrefix);

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
		/// <summary>
		/// Сокращенная запись для <code>ForPlugin&lt;TPlugin&gt;().UseInstance(value, contracts)</code>
		/// </summary>
		public static IPluginConfigurator<TPlugin> Bind<TPlugin>(this IContainerConfigurator configurator, TPlugin value, params ContractDeclaration[] contracts)
		{
			IPluginConfigurator<TPlugin> pluginConfigurator = configurator.ForPlugin<TPlugin>();
			pluginConfigurator.UseInstance(value, contracts);
			return pluginConfigurator;
		}

		/// <summary>
		/// Сокращенная запись для <code>ForPlugin&lt;TPlugin&gt;().UsePluggable&lt;TPluggable&gt;(contracts)</code>
		/// </summary>
		public static IPluginConfigurator<TPlugin> Bind<TPlugin, TPluggable>(this IContainerConfigurator configurator, params ContractDeclaration[] contracts) where TPluggable : TPlugin
		{
			IPluginConfigurator<TPlugin> pluginConfigurator = configurator.ForPlugin<TPlugin>();
			pluginConfigurator.UsePluggable<TPluggable>(contracts);
			return pluginConfigurator;
		}
	}

}