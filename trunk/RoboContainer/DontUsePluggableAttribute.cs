using System;

namespace RoboContainer
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
	public class DontUsePluggableAttribute : Attribute
	{
		public DontUsePluggableAttribute(Type ignoredPluggable)
		{
			IgnoredPluggable = ignoredPluggable;
		}

		public Type IgnoredPluggable { get; set; }
	}
}