using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace RoboConfig
{
	public class ArrayDeserializer : IDeserializer
	{
		private readonly XmlDeserializators xmlDeserializators;

		public ArrayDeserializer(XmlDeserializators xmlDeserializators)
		{
			this.xmlDeserializators = xmlDeserializators;
		}

		public bool CanDeserialize(Type type)
		{
			return type.IsArray;
		}

		public object Deserialize(Type type, XmlElement source, string name)
		{
			Debug.Assert(type.IsArray);
			List<object> items = source.ChildNodes.OfType<XmlElement>()
				.Where(e => e.Name == name)
				.Select(e => xmlDeserializators.Deserialize(e, "item", type.GetElementType())).ToList();
			var result = Array.CreateInstance(type.GetElementType(), items.Count());
			for(int i = 0; i < result.Length; i++)
				result.SetValue(items[i], i);
			Debug.Assert(result.GetType() == type);
			return result;
		}
	}
}