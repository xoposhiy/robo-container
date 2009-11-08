using System;

namespace RoboContainer
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class PluginAttribute : Attribute
	{
		private InstanceLifetime scope;
		public InstanceLifetime Scope
		{
			get { return scope; }
			set {
				ScopeSpecified = true;
				scope = value; }
		}

		public bool ScopeSpecified { get; private set; }

		public Type PluggableType { get; set; }
	}
}