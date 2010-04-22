using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
			ScanAssemblies(AssembliesUtils.GetTheCallingAssembly());
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
			configuration.AddScanner(scanner);
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

		public void ForceInjectionOf<TPlugin>(params ContractRequirement[] requiredContracts)
		{
			configuration.ForceInjectionOf(typeof(TPlugin), requiredContracts);
		}

		public void ScanLoadedCompanyAssemblies()
		{
			string callingAssemblyName = AssembliesUtils.GetTheCallingAssembly().FullName;
			ScanLoadedAssemblies(a => HasCommonDotPrefix(callingAssemblyName, a.FullName));
		}

		public void ScanLoadedAssembliesWithPrefix(string companyPrefix)
		{
			ScanLoadedAssemblies(a => a.FullName != null && a.FullName.StartsWith(companyPrefix));
		}

		// MyCompany.Something and MyCompany.Anything.Else has common dot-prefix
		// MyCompanyX and MyCompanyY has not.
		private static bool HasCommonDotPrefix(string name1, string name2)
		{
			int minLen = Math.Min(name1.Length, name2.Length);
			for (int i = 0; i < minLen; i++)
			{
				if (name1[i] != name2[i]) return false;
				if (name2[i] == '.') return true;
			}
			if (name1.Length > minLen && name1[minLen] == '.') return true; //MyCompany.Core and MyCompany
			if (name2.Length > minLen && name2[minLen] == '.') return true; //MyCompany and MyCompany.Core
			if (name1.Length == name2.Length) return true; //MyCompany.Core and MyCompany.Core
			return false; //MyCompany and MyCompanyXYZ
		}

		public IExternalConfigurator ConfigureBy
		{
			get { return new ExternalConfigurator(this); }
		}

		public void RegisterInitializer(params IPluggableInitializer[] initializers)
		{
			configuration.RegisterInitializer(initializers);
		}

		public ILoggingConfigurator Logging
		{
			get { return configuration.GetLoggingConfigurator(); }
		}

		public IConstructionLogger GetLogger()
		{
			return configuration.GetConfiguredLogging().GetLogger();
		}
	}
}