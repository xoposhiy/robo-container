namespace RoboContainer.Core
{
	public interface IExternalConfigurator
	{
		/// <summary>
		/// Конфигурирует контейнер на основе секции <paramref name="sectionName"/> из app.config-файла.
		/// Соответствующая секция должна быть... TODO
		/// </summary>
		void AppConfigSection(string sectionName);

		/// <summary>
		/// Конфигурирует контейнер на основе секции 'robocontainer' из app.config-файла.
		/// Соответствующая секция должна быть... TODO
		/// </summary>
		void AppConfig();
	
		/// <summary>
		/// Конфигурирует контейнер на основе секции Xml-файла <paramref name="filename"/>.
		/// </summary>
		void XmlFile(string filename);
	}
}