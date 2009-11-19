using System;
using RoboContainer.Impl;

namespace RoboContainer.Tests
{
	public class BaseContainerTest
	{
		public static ContainerConfiguration AfterConfigure(Type pluginType, Type pluggableType)
		{
			var containerConfiguration = new ContainerConfiguration();
			containerConfiguration.Configurator.ForPlugin(pluginType).PluggableIs(pluggableType);
			return containerConfiguration;
		}

		public static ContainerConfiguration ConfigureAndCheckThat(Type pluginType, Type pluggableType)
		{
			var containerConfiguration = new ContainerConfiguration();
			containerConfiguration.Configurator.ForPlugin(pluginType).PluggableIs(pluggableType);
			containerConfiguration.CheckThat(pluginType, pluggableType);
			return containerConfiguration;
		}

		public static ContainerConfiguration WithoutConfiguration()
		{
			return new ContainerConfiguration();
		}
	}
}