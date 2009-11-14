using System;
using System.Collections.Generic;
using System.Reflection;

namespace RoboContainer.Impl
{
	public interface IContainerConfiguration
	{
		bool HasAssemblies();
		IEnumerable<Type> GetScannableTypes();
		IConfiguredPluggable GetConfiguredPluggable(Type pluggableType);
		bool HasConfiguredPluggable(Type pluggableType);
		IConfiguredPlugin GetConfiguredPlugin(Type pluginType);
		bool HasConfiguredPlugin(Type pluginType);

		IContainerConfigurator Configurator { get; }
		void ScanAssemblies(IEnumerable<Assembly> assembliesToScan);
		IPluggableConfigurator GetPluggableConfigurator(Type pluggableType);
		IPluginConfigurator GetPluginConfigurator(Type pluginType);
	}
}