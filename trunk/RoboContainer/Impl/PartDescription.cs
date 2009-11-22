using System;

namespace RoboContainer.Impl
{
	public class PartDescription
	{
		public PartDescription(bool useOnlyThis, Type asPlugin, Func<object> part)
		{
			UseOnlyThis = useOnlyThis;
			AsPlugin = asPlugin;
			Part = part;
		}

		public bool UseOnlyThis { get; private set; }
		public Type AsPlugin { get; private set; }
		public Func<object> Part { get; private set; }
	}
}