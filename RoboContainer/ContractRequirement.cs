﻿namespace RoboContainer
{
	public class ContractRequirement
	{
		public static implicit operator ContractRequirement(string value)
		{
			return new NamedRequirement(value);
		}
	}
}