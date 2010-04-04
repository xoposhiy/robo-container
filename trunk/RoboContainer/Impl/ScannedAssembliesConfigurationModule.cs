namespace RoboContainer.Impl
{
	public class ScannedAssembliesConfigurationModule : IConfigurationModule
	{
		public void Configure(IContainerConfiguration configuration)
		{
			if(!configuration.HasAssemblies())
			{
				configuration.Configurator.ScanLoadedCompanyAssemblies();
				configuration.Configurator.ScanCallingAssembly();
			}
		}
	}
}
