using System;
using System.Collections.Generic;
using System.Reflection;

namespace RoboContainer
{
	public delegate void ScannerDelegate(IContainerConfigurator configurator, Type pluggableType);

	public interface IContainerConfigurator
	{
		void ScanAssemblies(params Assembly[] assembliesToScan);
		void ScanAssemblies(IEnumerable<Assembly> assembliesToScan);
		void ScanCallingAssembly();
		void ScanLoadedAssemblies();
		void ScanLoadedAssemblies(Func<Assembly, bool> shouldScan);

		void ScanTypesWith(ScannerDelegate scanner);

		IPluggableConfigurator ForPluggable(Type pluggableType);
		IPluggableConfigurator<TPluggable> ForPluggable<TPluggable>();

		IPluginConfigurator<TPlugin> ForPlugin<TPlugin>();
		IPluginConfigurator ForPlugin(Type pluginType);

		void AddPart(Type pluginType, object pluggable);
		void AddParts(Type pluginType, params object[] pluggable);

		void AddPart<TPluginType>(TPluginType pluggable);
		void AddParts<TPluginType>(params TPluginType[] pluggable);
	}
}