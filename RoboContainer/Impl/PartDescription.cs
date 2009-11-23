using System;

namespace RoboContainer.Impl
{
	public class PartDescription
	{
		public PartDescription(string name, bool useOnlyThis, Type asPlugin, Func<object> part)
		{
			Name = name;
			UseOnlyThis = useOnlyThis;
			AsPlugin = asPlugin;
			Part = part;
		}

		public string Name { get; private set; }
		public bool UseOnlyThis { get; private set; }
		public Type AsPlugin { get; private set; }
		public Func<object> Part { get; private set; }
	}
}