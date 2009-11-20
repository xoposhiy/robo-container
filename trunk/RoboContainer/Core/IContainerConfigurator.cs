using System;
using System.Collections.Generic;
using System.Reflection;

namespace RoboContainer.Core
{
	public delegate void ScannerDelegate(IContainerConfigurator configurator, Type type);

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

		void AddPart<TPluginType>(TPluginType pluggable);
		void AddParts(params object[] pluggable);
	}
}