using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace RoboConfig
{
	public class XmlDeserializators
	{
		private readonly IList<IDeserializer> deserializers = new List<IDeserializer>();

		public XmlDeserializators()
		{
			deserializers.Add(new DeserializerOf<string>(s => s));
			deserializers.Add(new DeserializerOf<int>(s => int.Parse(s, NumberFormatInfo.InvariantInfo)));
			deserializers.Add(new DeserializerOf<double>(s => double.Parse(s, NumberFormatInfo.InvariantInfo)));
			deserializers.Add(new DeserializerOf<float>(s => float.Parse(s, NumberFormatInfo.InvariantInfo)));
			deserializers.Add(new DeserializerOf<Type>(s => Type.GetType(s, true, false)));
			deserializers.Add(new EnumDeserializer());
			deserializers.Add(new ArrayDeserializer(this));
		}

		public object Deserialize(XmlElement source, string name, Type type)
		{
			IDeserializer deserializer = deserializers.FirstOrDefault(d => d.CanDeserialize(type));
			if(deserializer == null)
				throw new Exception("Can't deserialize type " + type);
			return deserializer.Deserialize(type, source, name);
		}

		public bool CanDeserialize(Type type)
		{
			return deserializers.Any(d => d.CanDeserialize(type));
		}
	}

	public class XmlActionsReader : IActionsReader<XmlElement>
	{
		public XmlDeserializators deserializator = new XmlDeserializators();

		public string ReadName(XmlElement source)
		{
			return source.Name;
		}

		public object ReadArg(XmlElement source, string name, Type type)
		{
			return deserializator.Deserialize(source, name, type);
		}

		public IEnumerable<XmlElement> ReadActions(XmlElement source)
		{
			return source.ChildNodes.OfType<XmlElement>().Where(child => char.IsUpper(child.Name, 0));
		}

		public bool CanDeserialize(Type type)
		{
			return deserializator.CanDeserialize(type);
		}
	}
}