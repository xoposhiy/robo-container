using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ChildConfiguration : ContainerConfiguration
	{
		private readonly IContainerConfiguration parent;
		private readonly Hashtable<Type, IConfiguredPluggable> pluggables = new Hashtable<Type, IConfiguredPluggable>();
		private readonly Hashtable<Type, IConfiguredPlugin> plugins = new Hashtable<Type, IConfiguredPlugin>();

		public ChildConfiguration(IContainerConfiguration parent)
		{
			this.parent = parent;
		}

		public override bool WasAssembliesExplicitlyConfigured
		{
			get { return base.WasAssembliesExplicitlyConfigured || parent.WasAssembliesExplicitlyConfigured; }
		}

		public override IConfiguredLogging GetConfiguredLogging()
		{
			return parent.GetConfiguredLogging();
		}

		public override ILoggingConfigurator GetLoggingConfigurator()
		{
			throw ContainerException.NoLog("Cant reconfigure parent logging");
		}

		public override void Dispose()
		{
			base.Dispose();
			pluggables.Dispose();
			plugins.Dispose();
		}

		public override IEnumerable<Type> GetScannableTypes()
		{
			return base.GetScannableTypes().Union(parent.GetScannableTypes());
		}

		public override IEnumerable<Type> GetScannableTypes(Type pluginType)
		{
			return base.GetScannableTypes(pluginType).Union(parent.GetScannableTypes(pluginType));
		}

		public override IConfiguredPluggable TryGetConfiguredPluggable(Type pluggableType)
		{
			return pluggables.GetOrCreate(pluggableType, CreateCombinedConfiguredPluggable);
		}

		private IConfiguredPluggable CreateCombinedConfiguredPluggable(Type pluggableType)
		{
			return new CombinedConfiguredPluggable(
				parent.TryGetConfiguredPluggable(pluggableType),
				base.TryGetConfiguredPluggable(pluggableType),
				this);
		}

		private IConfiguredPlugin CreateCombinedConfiguredPlugin(Type pluginType)
		{
			return new CombinedConfiguredPlugin(parent.GetConfiguredPlugin(pluginType),
			                                    base.GetConfiguredPlugin(pluginType),
			                                    this);
		}

		public override IConfiguredPlugin GetConfiguredPlugin(Type pluginType)
		{
			return plugins.GetOrCreate(pluginType, CreateCombinedConfiguredPlugin);
		}

		public IConfiguredPluggable GetChildConfiguredPluggable(IConfiguredPluggable pluggable)
		{
			return base.TryGetConfiguredPluggable(pluggable.PluggableType) ?? pluggable; //TODO разобраться с этой строкой.
		}
	}
}