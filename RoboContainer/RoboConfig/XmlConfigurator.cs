using System.Configuration;
using System.IO;
using System.Xml;

namespace RoboConfig
{
	public class XmlConfigurator : Configurator<XmlElement>
	{
		public XmlConfigurator(XmlElement rootElement)
			: base(new XmlActionsReader(), rootElement)
		{
		}

		public static XmlConfigurator FromStream(Stream xmlStream)
		{
			var doc = new XmlDocument();
			doc.Load(xmlStream);
			return new XmlConfigurator(doc.DocumentElement);
		}

		public static XmlConfigurator FromString(string xml)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			return new XmlConfigurator(doc.DocumentElement);
		}

		public static XmlConfigurator FromFile(string filename)
		{
			var doc = new XmlDocument();
			doc.Load(filename);
			return new XmlConfigurator(doc.DocumentElement);
		}

		public static XmlConfigurator FromAppConfig(string sectionName)
		{
			var section = (XmlElement) ConfigurationManager.GetSection(sectionName);
			return new XmlConfigurator(section);
		}
	}
}