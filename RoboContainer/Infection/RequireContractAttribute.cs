using System;

namespace RoboContainer.Infection
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
	public class RequireContractAttribute : Attribute
	{
		public RequireContractAttribute(params string[] contracts)
		{
			Contracts = contracts;
		}

		public string[] Contracts { get; private set; }
	}
}