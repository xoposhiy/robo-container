using System;
using System.Xml;

namespace RoboConfig
{
	public class EnumDeserializer : IDeserializer
	{
		public bool CanDeserialize(Type type)
		{
			return type.IsEnum;
		}

		public object Deserialize(Type type, XmlElement source, string name)
		{
			return Enum.Parse(type, source.GetAttribute(name));
		}
	}
}