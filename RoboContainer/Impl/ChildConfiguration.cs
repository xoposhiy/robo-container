﻿using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ChildConfiguration : ContainerConfiguration
	{
		private readonly IContainerConfiguration parent;
		private readonly IDictionary<Type, IConfiguredPluggable> pluggables = new Dictionary<Type, IConfiguredPluggable>();
		private readonly IDictionary<Type, IConfiguredPlugin> plugins = new Dictionary<Type, IConfiguredPlugin>();

		public ChildConfiguration(IContainerConfiguration parent)
		{
			this.parent = parent;
		}

		public override IConfiguredLogging GetConfiguredLogging()
		{
			return parent.GetConfiguredLogging();
		}

		public override object Lock
		{
			get { return parent.Lock; }
		}

		public override ILoggingConfigurator GetLoggingConfigurator()
		{
			throw ContainerException.NoLog("Cant reconfigure parent logging");
		}

		public override void Dispose()
		{
			base.Dispose();
			pluggables.Values.ForEach(c => c.Dispose());
			pluggables.Clear();
			plugins.Values.ForEach(c => c.Dispose());
			plugins.Clear();
		}

		public override bool WasAssembliesExplicitlyConfigured
		{
			get { return base.WasAssembliesExplicitlyConfigured || parent.WasAssembliesExplicitlyConfigured; }
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
			IConfiguredPluggable result;
			if(!pluggables.TryGetValue(pluggableType, out result))
			{
				result = new CombinedConfiguredPluggable(
					parent.TryGetConfiguredPluggable(pluggableType),
					base.TryGetConfiguredPluggable(pluggableType),
					this);
				pluggables.Add(pluggableType, result);
			}
			return result;
		}

		public override IConfiguredPlugin GetConfiguredPlugin(Type pluginType)
		{
			IConfiguredPlugin result;
			if(!plugins.TryGetValue(pluginType, out result))
			{
				result = new CombinedConfiguredPlugin(
					parent.GetConfiguredPlugin(pluginType),
					base.GetConfiguredPlugin(pluginType),
					this);
				plugins.Add(pluginType, result);
			}
			return result;
		}

		public IConfiguredPluggable GetChildConfiguredPluggable(IConfiguredPluggable pluggable)
		{
			return base.TryGetConfiguredPluggable(pluggable.PluggableType) ?? pluggable; //TODO разобраться с этой строкой.
		}
	}
}