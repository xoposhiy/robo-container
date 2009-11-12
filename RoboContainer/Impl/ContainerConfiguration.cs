using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using RoboContainer;

namespace RoboContainer.Impl
{
	public class ContainerConfiguration : IContainerConfigurator
	{
		private readonly List<Assembly> assemblies = new List<Assembly>();

		private readonly IDictionary<Type, PluggableConfigurator> pluggableConfigs =
			new Dictionary<Type, PluggableConfigurator>();

		private readonly IDictionary<Type, PluginConfigurator> pluginConfigs = new Dictionary<Type, PluginConfigurator>();

		public void ScanAssemblies(params Assembly[] assembliesToScan)
		{
			ScanAssemblies((IEnumerable<Assembly>) assembliesToScan);
		}

		public void ScanAssemblies(IEnumerable<Assembly> assembliesToScan)
		{
			assemblies.AddRange(assembliesToScan);
		}

		public void ScanCallingAssembly()
		{
			ScanAssemblies(GetTheCallingAssembly());
		}

		public void ScanLoadedAssemblies()
		{
			ScanLoadedAssemblies(assembly => true);
		}

		public void ScanLoadedAssemblies(Func<Assembly, bool> shouldScan)
		{
			ScanAssemblies(AppDomain.CurrentDomain.GetAssemblies().Where(shouldScan).ToArray());
		}

		public void ScanTypesWith(ScannerDelegate scanner)
		{
			foreach (Type type in GetScannableTypes())
				scanner(this, type);
		}

		public IPluggableConfigurator ForPluggable(Type pluggableType)
		{
			return GetPluggableConfigurator(pluggableType);
		}

		public IPluggableConfigurator<TPluggable> ForPluggable<TPluggable>()
		{
			return ForPluggable(typeof (TPluggable)).TypedConfigurator<TPluggable>();
		}

		public IPluginConfigurator<TPlugin> ForPlugin<TPlugin>()
		{
			return ForPlugin(typeof (TPlugin)).TypedConfigurator<TPlugin>();
		}

		public IPluginConfigurator ForPlugin(Type pluginType)
		{
			return pluginConfigs.GetOrCreate(pluginType, () => new PluginConfigurator(this, pluginType));
		}

		public void AddPart(Type pluginType, object pluggable)
		{
			if (pluginConfigs.ContainsKey(pluginType) && pluginConfigs[pluginType].CreatePluggable != null)
				throw new ContainerException("Many parts for one plugin is not supported yet");
			ForPlugin(pluginType).CreatePluggableBy((c, pluginType_not_used) => pluggable);
			AddPartsExportedBy(pluggable);
		}

		public void AddParts(Type pluginType, params object[] pluggables)
		{
			foreach (object pluggable in pluggables) AddPart(pluggable);
		}

		public void AddPart<TPluginType>(TPluginType pluggable)
		{
			AddPart(typeof (TPluginType), pluggable);
		}

		public void AddParts<TPluginType>(params TPluginType[] pluggables)
		{
			foreach (TPluginType pluggable in pluggables) AddPart(pluggable);
		}

		public IEnumerable<Type> GetScannableTypes()
		{
			if (!assemblies.Any())
				ScanCallingAssembly();
			return assemblies.SelectMany(assembly => assembly.GetExportedTypes());
		}

		private void AddPartsExportedBy(object pluggable)
		{
			var exportableProperties = pluggable.GetType().GetProperties().Where(p => p.HasAttribute<ExportedPartAttribute>());
			foreach (var prop in exportableProperties)
			{
				AddPart(
					prop.GetAttribute<ExportedPartAttribute>().AsPlugin ?? prop.PropertyType,
					prop.GetValue(pluggable, null));
			}
		}

		// use
		public IConfiguredPlugin GetConfiguredPlugin(Type pluginType)
		{
			return pluginConfigs.GetOrCreate(pluginType, () => GetPluginConfiguratorWithoutCache(pluginType));
		}

		// use
		public IConfiguredPluggable GetConfiguredPluggable(Type pluggableType)
		{
			return GetPluggableConfigurator(pluggableType);
		}

		// config & use
		private PluggableConfigurator GetPluggableConfigurator(Type pluggableType)
		{
			return pluggableConfigs.GetOrCreate(pluggableType, () => PluggableConfigurator.FromAttributes(pluggableType));
		}

		private static Assembly GetTheCallingAssembly()
		{
			Assembly thisAssembly = Assembly.GetExecutingAssembly();
			StackFrame[] frames = new StackTrace(false).GetFrames() ?? new StackFrame[0];
			return
				frames
					.Select(f => f.GetMethod().DeclaringType.Assembly)
					.First(a => a != thisAssembly);
		}

		private PluginConfigurator GetPluginConfiguratorWithoutCache(Type pluginType)
		{
			PluginConfigurator configuredPlugin;
			if (pluginType.IsGenericType &&
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

		public bool HasAssemblies()
		{
			return assemblies.Any();
		}
	}
}