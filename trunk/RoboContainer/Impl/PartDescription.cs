using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class PartDescription
	{
		public PartDescription(string name, bool useOnlyThis, Type asPlugin, Func<object> part, string[] declaredContracts)
		{
			Name = name;
			UseOnlyThis = useOnlyThis;
			AsPlugin = asPlugin;
			Part = part;
			DeclaredContracts = declaredContracts;
		}

		public string Name { get; private set; }
		public bool UseOnlyThis { get; private set; }
		public Type AsPlugin { get; private set; }
		public Func<object> Part { get; private set; }
		public string[] DeclaredContracts { get; private set; }
	}
}