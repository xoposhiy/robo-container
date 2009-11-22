using System;
using RoboContainer.Core;

namespace RoboContainer.Infection
{
	[AttributeUsage(AttributeTargets.Class)]
	public class PluggableAttribute : Attribute
	{
		public LifetimeScopeEnum Lifetime { get; set; }
	}
}