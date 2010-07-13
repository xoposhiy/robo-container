using System;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Infection
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
	public class UsePluggableAttribute : Attribute
	{
		public UsePluggableAttribute(Type pluggableType, params string[] declaredContracts)
		{
			PluggableType = pluggableType;
			DeclaredContracts = declaredContracts.Select(c => (string)c).ToArray();
		}

		public UsePluggableAttribute(Type pluggableType)
		{
			PluggableType = pluggableType;
			DeclaredContracts = new string[0];
		}

		public UsePluggableAttribute(Type pluggableType, params Type[] declaredContracts)
		{
			PluggableType = pluggableType;
			DeclaredContracts = declaredContracts.Select(c => c.Name).ToArray();
		}

		public Type PluggableType { get; private set; }
		public string[] DeclaredContracts { get; set; }
	}
}