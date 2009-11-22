using System;

namespace RoboContainer.Infection
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ProvidePartAttribute : Attribute
	{
		public Type AsPlugin { get; set; }

		public bool UseOnlyThis { get; set; }
	}
}