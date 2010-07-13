using System;

namespace RoboContainer.Infection
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class InjectAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class DontInjectAttribute : Attribute
	{
	}
}