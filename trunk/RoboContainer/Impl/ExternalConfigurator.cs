using RoboConfig;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ExternalConfigurator : IExternalConfigurator
	{
		private readonly ContainerConfigurator configurator;

		public ExternalConfigurator(ContainerConfigurator configurator)
		{
			this.configurator = configurator;
		}

		public void AppConfigSection(string sectionName)
		{
			XmlConfigurator.FromAppConfig(sectionName).ApplyConfigTo(configurator);
		}

		public void AppConfig()
		{
			AppConfigSection("robocontainer");
		}

		public void XmlFile(string filename)
		{
			XmlConfigurator.FromFile(filename).ApplyConfigTo(configurator);
		}
	}
}