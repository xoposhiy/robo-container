using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public class ContainerConfigurator : IContainerConfigurator
	{
		private readonly IContainerConfiguration configuration;

		public ContainerConfigurator(IContainerConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public void ScanAssemblies(params Assembly[] assembliesToScan)
		{
			ScanAssemblies((IEnumerable<Assembly>) assembliesToScan);
		}

		public void ScanAssemblies(IEnumerable<Assembly> assembliesToScan)
		{
			configuration.ScanAssemblies(assembliesToScan);
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
			foreach (Type type in configuration.GetScannableTypes())
				scanner(this, type);
		}

		public IPluggableConfigurator ForPluggable(Type pluggableType)
		{
			return configuration.GetPluggableConfigurator(pluggableType);
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
			return configuration.GetPluginConfigurator(pluginType);
		}

		public void AddPart(Type pluginType, object pluggable)
		{
			ForPlugin(pluginType).CreatePluggableBy((c, pluginType_not_used) => pluggable);
			AddPartsExportedBy(pluggable);
		}

		public void AddPart<TPluginType>(TPluginType pluggable)
		{
			AddPart(typeof (TPluginType), pluggable);
		}

		public void AddParts(params object[] pluggables)
		{
			foreach (object pluggable in pluggables) AddPart(pluggable);
		}

		private void AddPartsExportedBy(object pluggable)
		{
			IEnumerable<PropertyInfo> exportableProperties = 
				pluggable.GetType().GetProperties().Where(p => p.HasAttribute<ExportedPartAttribute>() && p.CanRead);
			foreach (PropertyInfo prop in exportableProperties)
			{
				AddPart(
					prop.GetAttribute<ExportedPartAttribute>().AsPlugin ?? prop.PropertyType,
					prop.GetValue(pluggable, null));
			}
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
	}
}