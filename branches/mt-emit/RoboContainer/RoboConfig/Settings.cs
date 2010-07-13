using System.Configuration;
using System.Xml;

namespace RoboContainer.RoboConfig
{
	public class Settings : IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, XmlNode section)
		{
			return section;
		}
	}
}
