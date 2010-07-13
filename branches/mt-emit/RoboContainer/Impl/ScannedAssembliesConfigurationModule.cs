namespace RoboContainer.Impl
{
	public class ScannedAssembliesConfigurationModule : IConfigurationModule
	{
		public void Configure(IContainerConfiguration configuration)
		{
			if(!configuration.WasAssembliesExplicitlyConfigured)
			{
				configuration.Configurator.ScanLoadedCompanyAssemblies();
				configuration.Configurator.ScanCallingAssembly();
			}
		}
	}
}
