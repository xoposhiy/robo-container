using System;
using System.Xml;

namespace RoboConfig
{
	public interface IDeserializer
	{
		bool CanDeserialize(Type type);
		object Deserialize(Type type, XmlElement source, string name);
	}
}