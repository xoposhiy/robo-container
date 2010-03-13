﻿using System;
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
		private TypesMap typesMap;

		public IConfiguredLogging GetConfiguredLogging()
		{
			return loggerConfigurator;
		}

		public void ScanAssemblies(IEnumerable<Assembly> assembliesToScan)
		{
			assembliesToScan.Exclude(assemblies.Contains).ForEach(assemblies.Add);
		}

		public virtual IEnumerable<Type> GetScannableTypes()
		{
			return assemblies.SelectMany(assembly => assembly.GetExportedTypes());
		}

		public virtual IEnumerable<Type> GetScannableTypes(Type pluginType)
		{
			if(pluginType.IsGenericType && !pluginType.IsGenericTypeDefinition)
				return GetScannableTypes(pluginType.GetGenericTypeDefinition());
			return
				(typesMap ?? (typesMap = new TypesMap(GetScannableTypes())))
					.GetInheritors(pluginType);
		}

		public virtual IPluginConfigurator GetPluginConfigurator(Type pluginType)
		{
			return pluginConfigs.GetOrCreate(pluginType, () => PluginConfigurator.FromAttributes(this, pluginType));
		}

		public virtual ILoggingConfigurator GetLoggingConfigurator()
		{
			return loggerConfigurator;
		}

		// use
		public IConfiguredPluggable TryGetConfiguredPluggable(Type pluginType, Type pluggableType)
		{
			if(!pluggableType.Constructable()) return null;
			pluggableType = GenericTypes.TryCloseGenericTypeToMakeItAssignableTo(pluggableType, pluginType);
			if(pluggableType == null) return null;
			if(pluggableType.ContainsGenericParameters) throw new DeveloperMistake(pluggableType);
			return TryGetConfiguredPluggable(pluggableType);
		}

		public virtual bool HasConfiguredPluggable(Type pluggableType)
		{
			return pluggableConfigs.ContainsKey(pluggableType);
		}

		public virtual IConfiguredPlugin GetConfiguredPlugin(Type pluginType)
		{
			return pluginConfigs.GetOrCreate(pluginType, () => GetPluginConfiguratorWithoutCache(pluginType));
		}

		public virtual bool HasConfiguredPlugin(Type pluginType)
		{
			return pluginConfigs.ContainsKey(pluginType) || pluggableConfigs.ContainsKey(pluginType);
		}

		// use
		public virtual IConfiguredPluggable TryGetConfiguredPluggable(Type pluggableType)
		{
			return pluggableConfigs.GetOrCreate(pluggableType, () => GetPluggableConfiguratorWithoutCache(pluggableType));
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
			return pluggableConfigs.GetOrCreate(pluggableType, () => GetPluggableConfiguratorWithoutCache(pluggableType));
		}

		public virtual void Dispose()
		{
			pluggableConfigs.Values.ForEach(c => c.Dispose());
			pluggableConfigs.Clear();
			pluginConfigs.Values.ForEach(c => c.Dispose());
			pluginConfigs.Clear();
		}

		private PluggableConfigurator GetPluggableConfiguratorWithoutCache(Type pluggableType)
		{
			PluggableConfigurator configuredPluggable;
			if(pluggableType.IsGenericType &&
				pluggableConfigs.TryGetValue(pluggableType.GetGenericTypeDefinition(), out configuredPluggable))
				return new PluggableConfigurator(pluggableType, configuredPluggable, this);
			return PluggableConfigurator.FromAttributes(pluggableType, this);
		}

		private PluginConfigurator GetPluginConfiguratorWithoutCache(Type pluginType)
		{
			PluginConfigurator configuredPlugin;
			if(pluginType.IsGenericType &&
				pluginConfigs.TryGetValue(pluginType.GetGenericTypeDefinition(), out configuredPlugin))
				return PluginConfigurator.FromGenericDefinition(configuredPlugin, this, pluginType);
			return PluginConfigurator.FromAttributes(this, pluginType);
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