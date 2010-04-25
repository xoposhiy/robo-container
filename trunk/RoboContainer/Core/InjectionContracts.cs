using System;
using System.Linq;

namespace RoboContainer.Core
{
	public static class InjectionContracts
	{
		public static bool IsContractAttribute(object attribute)
		{
			return IsContractAttribute(attribute.GetType());
		}

		public static bool IsContractAttribute(Type attributeType)
		{
			return attributeType.GetCustomAttributes(true).Any(a => MeansRequiredContractAttribute(a.GetType()));
		}

		public static bool MeansRequiredContractAttribute(Type attributeType)
		{
			var typename = attributeType.Name;
			return typename == "MeansInjectionContractAttribute" || typename == "MeansInjectionContract";
		}
	}
}