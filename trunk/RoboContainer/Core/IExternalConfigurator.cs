namespace RoboContainer.Core
{
	public interface IExternalConfigurator
	{
		/// <summary>
		/// Конфигурирует контейнер на основе секции <paramref name="sectionName"/> из app.config-файла.
		/// Секции должен быть сопоставлен обработчик "RoboContainer.RoboConfig.Settings, RoboContainer".
		/// <para>
		/// Пример файла app.config
		/// <code>
		/// <![CDATA[
		/// <configuration>
		///		<configSections>
		/// 		<section name="robo" type="RoboContainer.RoboConfig.Settings, RoboContainer"/>
		/// 	</configSections>
		/// 	<roboconfig xmlns="http://robo-container.googlecode.com/roboconfig">
		/// 		<ForPlugin pluginType="TestAppConfig.IFoo, TestAppConfig">
		///				<UsePluggable pluggableType="TestAppConfig.Foo2, TestAppConfig" />
		/// 		</ForPlugin>
		/// 	</roboconfig>
		/// </configuration>
		/// ]]>
		/// </code>
		/// </para>
		/// <seealso cref="AppConfig"/>
		/// <seealso cref="XmlFile"/>
		/// </summary>
		void AppConfigSection(string sectionName);

		/// <summary>
		/// Конфигурирует контейнер на основе секции 'robocontainer' из app.config-файла.
		/// Аналог вызова <c>AppConfigSection("roboconfig")</c>
		/// </summary>
		/// <seealso cref="AppConfigSection"/>
		/// <seealso cref="XmlFile"/>
		void AppConfig();
	
		/// <summary>
		/// Конфигурирует контейнер на основе секции Xml-файла <paramref name="filename"/>.
		/// <para>Пример xml-файла
		/// <code>
		/// <![CDATA[
		/// 	<roboconfig xmlns="http://robo-container.googlecode.com/roboconfig">
		/// 		<ForPlugin pluginType="TestAppConfig.IFoo, TestAppConfig">
		///				<UsePluggable pluggableType="TestAppConfig.Foo2, TestAppConfig" />
		/// 		</ForPlugin>
		/// 	</roboconfig>
		/// ]]>
		/// </code>
		/// </para>
		/// <para>
		/// Синтаксис xml полностью повторяет синтаксис конфигурирования кодом.
		/// </para>
		/// <seealso cref="AppConfig"/>
		/// <seealso cref="AppConfigSection"/>
		/// </summary>
		void XmlFile(string filename);
	}
}