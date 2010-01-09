using System;
using System.Collections.Generic;
using System.Linq;

namespace RoboContainer.Impl
{
	public class ScopedConfiguration : ContainerConfiguration
	{
		private readonly IContainerConfiguration parent;

		public ScopedConfiguration(IContainerConfiguration parent)
		{
			this.parent = parent;
		}

		public override bool HasAssemblies()
		{
			return base.HasAssemblies() || parent.HasAssemblies();
		}

		public override IEnumerable<Type> GetScannableTypes()
		{
			return base.GetScannableTypes().Union(parent.GetScannableTypes());
		}

		public override IEnumerable<Type> GetScannableTypes(Type pluginType)
		{
			return base.GetScannableTypes(pluginType).Union(parent.GetScannableTypes(pluginType));
		}

		public override bool HasConfiguredPluggable(Type pluggableType)
		{
			return base.HasConfiguredPluggable(pluggableType) || parent.HasConfiguredPluggable(pluggableType);
		}

		public override bool HasConfiguredPlugin(Type pluginType)
		{
			return base.HasConfiguredPlugin(pluginType) || parent.HasConfiguredPlugin(pluginType);
		}

		public override IConfiguredPluggable GetConfiguredPluggable(Type pluggableType)
		{
			if(base.HasConfiguredPluggable(pluggableType)) return base.GetConfiguredPluggable(pluggableType);
			return parent.GetConfiguredPluggable(pluggableType);
		}

		public override IConfiguredPlugin GetConfiguredPlugin(Type pluginType)
		{
			if(base.HasConfiguredPlugin(pluginType)) return base.GetConfiguredPlugin(pluginType);
			return parent.GetConfiguredPlugin(pluginType);
		}
	}
}