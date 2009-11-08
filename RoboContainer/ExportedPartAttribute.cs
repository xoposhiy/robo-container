using System;

namespace RoboContainer
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ExportedPartAttribute : Attribute
	{
		public Type AsPlugin { get; set; }
	}
}