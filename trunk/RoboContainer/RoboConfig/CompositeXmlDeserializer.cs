using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using RoboContainer.RoboConfig;

namespace RoboConfig
{
	public class CompositeXmlDeserializer : IDeserializer
	{
		private readonly IList<IDeserializer> deserializers = new List<IDeserializer>();

		public CompositeXmlDeserializer()
		{
			deserializers.Add(new DeserializerOf<string>(s => s));
			deserializers.Add(new DeserializerOf<int>(s => int.Parse(s, NumberFormatInfo.InvariantInfo)));
			deserializers.Add(new DeserializerOf<double>(s => double.Parse(s, NumberFormatInfo.InvariantInfo)));
			deserializers.Add(new DeserializerOf<float>(s => float.Parse(s, NumberFormatInfo.InvariantInfo)));
			deserializers.Add(new DeserializerOf<bool>(s => bool.Parse(s)));
			deserializers.Add(new DeserializerOf<Type>(s => Type.GetType(s, true, false)));
			deserializers.Add(new EnumDeserializer());
			deserializers.Add(new ArrayDeserializer(this));
			deserializers.Add(new ConvertableFromStringDeserializer());
			deserializers.Add(new ObjectDeserializer());
		}

		public object Deserialize(Type type, XmlElement source, string name)
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
}