using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace RoboConfig
{
	public class XmlActionsReader : IActionsReader<XmlElement>
	{
		private readonly IDeserializer deserializer = new CompositeXmlDeserializer();

		public string ReadName(XmlElement source)
		{
			return source.Name;
		}

		public object ReadArg(XmlElement source, string name, Type type)
		{
			return deserializer.Deserialize(type, source, name);
		}

		public IEnumerable<XmlElement> ReadActions(XmlElement source)
		{
			return source.ChildNodes.OfType<XmlElement>().Where(child => char.IsUpper(child.Name, 0));
		}

		public bool CanDeserialize(Type type)
		{
			return deserializer.CanDeserialize(type);
		}
	}
}