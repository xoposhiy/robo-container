using System;
using System.Xml;

namespace RoboConfig
{
	public class DeserializerOf<TType> : IDeserializer
	{
		public DeserializerOf(Func<string, TType> deserialize)
		{
			this.deserialize = deserialize;
		}

		private readonly Func<string, TType> deserialize;

		public bool CanDeserialize(Type type)
		{
			return type == typeof(TType);
		}

		public object Deserialize(Type type, XmlElement source, string name)
		{
			if (!source.HasAttribute(name))
				throw new Exception("Отсутствует атрибут " + name + ". Узел:\r\n" + source.OuterXml);
			return deserialize(source.GetAttribute(name));
		}
	}
}