using System;
using RoboContainer.Core;

namespace RoboContainer.Infection
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class PluginAttribute : Attribute
	{
		private ReusePolicy reusePluggable;

		public ReusePolicy ReusePluggable
		{
			get { return reusePluggable; }
			set
			{
				ReusePolicySpecified = true;
				reusePluggable = value;
			}
		}

		public bool ReusePolicySpecified { get; private set; }

		public Type PluggableType { get; set; }
	}
}