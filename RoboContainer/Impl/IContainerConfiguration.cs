using System;
using System.Collections.Generic;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IContainerConfiguration : IDisposable
	{
		IDisposable StartResolve(Type t, ContractRequirement[] contracts);
		IContainerConfigurator Configurator { get; }
		bool WasAssembliesExplicitlyConfigured { get; }
		IEnumerable<Type> GetScannableTypes();
		IEnumerable<Type> GetScannableTypes(Type pluginType);
		[CanBeNull]
		IConfiguredPluggable TryGetConfiguredPluggable(Type pluggableType);
		[CanBeNull]
		IConfiguredPluggable TryGetConfiguredPluggable(Type pluginType, Type pluggableType);
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