using System;
using RoboContainer.Core;

namespace RoboContainer.Infection
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class PluginAttribute : Attribute
	{
		private LifetimeScopeEnum lifetime;

		public LifetimeScopeEnum Lifetime
		{
			get { return lifetime; }
			set
			{
				ScopeSpecified = true;
				lifetime = value;
			}
		}

		public bool ScopeSpecified { get; private set; }

		public Type PluggableType { get; set; }
	}
}