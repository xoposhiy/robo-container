using System;
using System.Collections.Generic;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IContainerConfiguration
	{
		IContainerConfigurator Configurator { get; }
		bool HasAssemblies();
		IEnumerable<Type> GetScannableTypes();
		IConfiguredPluggable GetConfiguredPluggable(Type pluggableType);
		bool HasConfiguredPluggable(Type pluggableType);
		IConfiguredPlugin GetConfiguredPlugin(Type pluginType);
		bool HasConfiguredPlugin(Type pluginType);
		IConfiguredLogging GetConfiguredLogging();

		void ScanAssemblies(IEnumerable<Assembly> assembliesToScan);
		IPluggableConfigurator GetPluggableConfigurator(Type pluggableType);
		IPluginConfigurator GetPluginConfigurator(Type pluginType);
		ILoggingConfigurator GetLoggingConfigurator();
	}
}