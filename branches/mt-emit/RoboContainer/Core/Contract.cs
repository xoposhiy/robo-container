using System;

namespace RoboContainer.Core
{
	public static class Contract
	{
		public const string Default = "DEFAULT";
		public const string Any = "*";
		public static bool Satisfy(this string contractRequirement, string contractDeclaration)
		{
			if (contractRequirement == Any || contractDeclaration == Any) return true;
			return contractRequirement.Equals(contractDeclaration, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
