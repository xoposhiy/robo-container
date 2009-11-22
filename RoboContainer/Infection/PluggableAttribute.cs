using System;
using RoboContainer.Core;

namespace RoboContainer.Infection
{
	[AttributeUsage(AttributeTargets.Class)]
	public class PluggableAttribute : Attribute
	{
		public LifetimeScope Lifetime { get; set; }
	}
}