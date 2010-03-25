using System.Globalization;
using RoboConfig;

namespace RoboContainer.RoboConfig
{
	public class ObjectDeserializer : DeserializerOf<object>
	{
		public ObjectDeserializer() : base(DeserializeObject)
		{
		}

		private static object DeserializeObject(string arg)
		{
			var indexOfColon = arg.IndexOf(":");
			if(indexOfColon != -1 && indexOfColon != arg.Length-1)
			{
				var type = arg.Substring(0, indexOfColon);
				var value = arg.Substring(indexOfColon+1);
				if(type == "bool") return bool.Parse(value);
				if(type == "int") return int.Parse(value, NumberFormatInfo.InvariantInfo);
			}
			return arg;
		}
	}
}