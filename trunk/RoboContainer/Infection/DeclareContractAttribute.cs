using System;

namespace RoboContainer.Infection
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DeclareContractAttribute : Attribute
	{
		public DeclareContractAttribute(params string[] contracts)
		{
			Contracts = contracts;
		}

		public string[] Contracts { get; private set; }
	}
}