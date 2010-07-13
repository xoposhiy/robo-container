using System;
using System.Xml;
using RoboConfig;

namespace RoboContainer.RoboConfig
{
	public class ConvertableFromStringDeserializer : IDeserializer
	{
		public bool CanDeserialize(Type type)
		{
			return type.GetMethod("op_Implicit", new[] { typeof(string) }) != null;
		}

		public object Deserialize(Type type, XmlElement source, string name)
		{
			if(!source.HasAttribute(name))
				throw new Exception("Отсутствует атрибут " + name + ". Узел:\r\n" + source.OuterXml);
			return type.GetMethod("op_Implicit", new[] {typeof(string)}).Invoke(null, new object[]{source.GetAttribute(name)});
		}
	}
}