using System;

namespace RoboContainer
{
	[AttributeUsage(AttributeTargets.Class)]
	public class PluggableAttribute : Attribute
	{
		public InstanceLifetime Scope { get; set; }
	}
}