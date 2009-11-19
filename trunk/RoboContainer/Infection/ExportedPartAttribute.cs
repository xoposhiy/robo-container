using System;

namespace RoboContainer.Infection
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ExportedPartAttribute : Attribute
	{
		public Type AsPlugin { get; set; }
	}
}