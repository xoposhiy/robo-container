using System.Configuration;
using System.IO;
using System.Xml;

namespace RoboConfig
{
	public class XmlConfiguration : Configuration<XmlElement>
	{
		public XmlConfiguration(XmlElement rootElement)
			: base(new XmlActionsReader(), rootElement)
		{
		}

		public static XmlConfiguration FromStream(Stream xmlStream)
		{
			var doc = new XmlDocument();
			doc.Load(xmlStream);
			return new XmlConfiguration(doc.DocumentElement);
		}

		public static XmlConfiguration FromString(string xml)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			return new XmlConfiguration(doc.DocumentElement);
		}

		public static XmlConfiguration FromFile(string filename)
		{
			var doc = new XmlDocument();
			doc.Load(filename);
			return new XmlConfiguration(doc.DocumentElement);
		}

		public static XmlConfiguration FromAppConfig(string sectionName)
		{
			var section = (XmlElement) ConfigurationManager.GetSection(sectionName);
			if(section == null) throw new ConfigurationErrorsException("Section " + sectionName + " not found");
			return new XmlConfiguration(section);
		}
	}
}