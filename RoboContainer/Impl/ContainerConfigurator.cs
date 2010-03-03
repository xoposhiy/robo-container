using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using RoboConfig;
using RoboContainer.Core;

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
			ScanAssemblies(AppDomain.CurrentDomain.GetAssemblies().Where(shouldScan).ToList());
		}

		public void ScanTypesWith(ScannerDelegate scanner)
		{
			configuration.GetScannableTypes().ForEach(type => scanner(this, type));
		}

		public IPluggableConfigurator ForPluggable(Type pluggableType)
		{
			return configuration.GetPluggableConfigurator(pluggableType);
		}

		public IPluggableConfigurator<TPluggable> ForPluggable<TPluggable>()
		{
			return ForPluggable(typeof(TPluggable)).TypedConfigurator<TPluggable>();
		}

		public IPluginConfigurator<TPlugin> ForPlugin<TPlugin>()
		{
			return ForPlugin(typeof(TPlugin)).TypedConfigurator<TPlugin>();
		}

		public IPluginConfigurator ForPlugin(Type pluginType)
		{
			return configuration.GetPluginConfigurator(pluginType);
		}

		public IExternalConfigurator ConfigureBy
		{
			get { return new ExternalConfigurator(this); }
		}

		public ILoggingConfigurator Logging
		{
			get { return configuration.GetLoggingConfigurator(); }
		}

		public IConstructionLogger GetLogger()
		{
			return configuration.GetConfiguredLogging().GetLogger();
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

	public class ExternalConfigurator : IExternalConfigurator
	{
		private readonly ContainerConfigurator configurator;

		public ExternalConfigurator(ContainerConfigurator configurator)
		{
			this.configurator = configurator;
		}

		public void AppConfigSection(string sectionName)
		{
			XmlConfigurator.FromAppConfig(sectionName).ApplyConfigTo(configurator);
		}

		public void AppConfig()
		{
			AppConfigSection("robocontainer");
		}

		public void XmlFile(string filename)
		{
			XmlConfigurator.FromFile(filename).ApplyConfigTo(configurator);
		}
	}
}