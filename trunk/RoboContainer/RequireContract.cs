using System;

namespace RoboContainer
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
	public class RequireContract : Attribute
	{
		public RequireContract(params string[] contracts)
		{
			Contracts = contracts;
		}

		public string[] Contracts { get; private set; }
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DeclareContract : Attribute
	{
		public DeclareContract(params string[] contracts)
		{
			Contracts = contracts;
		}

		public string[] Contracts { get; private set; }
	}
}