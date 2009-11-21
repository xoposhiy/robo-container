using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ContainerConfiguration : IContainerConfiguration
	{
		private readonly List<Assembly> assemblies = new List<Assembly>();
		private readonly LoggingConfigurator loggerConfigurator = new LoggingConfigurator();

		private readonly IDictionary<Type, PluggableConfigurator> pluggableConfigs =
			new Dictionary<Type, PluggableConfigurator>();

		private readonly IDictionary<Type, PluginConfigurator> pluginConfigs = new Dictionary<Type, PluginConfigurator>();

		public IConfiguredLogging GetConfiguredLogging()
		{
			return loggerConfigurator;
		}

		public void ScanAssemblies(IEnumerable<Assembly> assembliesToScan)
		{
			assemblies.AddRange(assembliesToScan);
		}

		public virtual IEnumerable<Type> GetScannableTypes()
		{
			return assemblies.SelectMany(assembly => assembly.GetExportedTypes());
		}

		public virtual IPluginConfigurator GetPluginConfigurator(Type pluginType)
		{
			return GetPluginConfigurator_Internal(pluginType);
		}

		public virtual ILoggingConfigurator GetLoggingConfigurator()
		{
			return loggerConfigurator;
		}

		// use
		public virtual bool HasConfiguredPluggable(Type pluggableType)
		{
			return pluggableConfigs.ContainsKey(pluggableType);
		}

		public virtual IConfiguredPlugin GetConfiguredPlugin(Type pluginType)
		{
			return GetPluginConfigurator_Internal(pluginType);
		}

		public virtual bool HasConfiguredPlugin(Type pluginType)
		{
			return pluginConfigs.ContainsKey(pluginType);
		}

		// use
		public virtual IConfiguredPluggable GetConfiguredPluggable(Type pluggableType)
		{
			return GetPluggableConfigurator_Internal(pluggableType);
		}

		public virtual bool HasAssemblies()
		{
			return assemblies.Any();
		}

		public IContainerConfigurator Configurator
		{
			get { return new ContainerConfigurator(this); }
		}

		// config & use
		public virtual IPluggableConfigurator GetPluggableConfigurator(Type pluggableType)
		{
			return GetPluggableConfigurator_Internal(pluggableType);
		}

		private PluginConfigurator GetPluginConfigurator_Internal(Type pluginType)
		{
			return pluginConfigs.GetOrCreate(pluginType, () => GetPluginConfiguratorWithoutCache(pluginType));
		}

		private PluggableConfigurator GetPluggableConfigurator_Internal(Type pluggableType)
		{
			return pluggableConfigs.GetOrCreate(pluggableType, () => PluggableConfigurator.FromAttributes(pluggableType));
		}

		private PluginConfigurator GetPluginConfiguratorWithoutCache(Type pluginType)
		{
			PluginConfigurator configuredPlugin;
			if(pluginType.IsGenericType &&
			   pluginConfigs.TryGetValue(pluginType.GetGenericTypeDefinition(), out configuredPlugin))
				return GetConfiguredPluginByClosingOpenGenericWithoutCache(configuredPlugin, pluginType);
			return PluginConfigurator.FromAttributes(this, pluginType);
		}

		private PluginConfigurator GetConfiguredPluginByClosingOpenGenericWithoutCache(PluginConfigurator configuredPlugin,
		                                                                               Type pluginType)
		{
			return PluginConfigurator.FromGenericDefinition(configuredPlugin, this, pluginType);
		}

		public class Part
		{
			public readonly object[] Pluggables;
			public readonly Type PluginType;

			public Part(Type pluginType, params object[] pluggables)
			{
				PluginType = pluginType;
				Pluggables = pluggables;
			}
		}
	}
}