using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Tests
{
	public static class ContainerTestingExtensions
	{
		public static ContainerConfiguration Configure(
			this ContainerConfiguration configuration, Type pluginType,
			Type pluggableType)
		{
			configuration.Configurator.ForPlugin(pluginType).PluggableIs(pluggableType);
			return configuration;
		}

		public static ContainerConfiguration CheckThat(
			this ContainerConfiguration configuration, Type requestedPluginType,
			params Type[] expectedPluggableTypes)
		{
			var container = new Container(configuration);
			if (expectedPluggableTypes.Length == 1)
				container.Get(requestedPluginType).ShouldBeInstanceOf(expectedPluggableTypes.Single());
			else
			{
				IEnumerable<object> pluggables = container.GetAll(requestedPluginType);
				if (expectedPluggableTypes.Length == 0)
					pluggables.ShouldBeEmpty();
				else
				{
					foreach (Type expectedPluggableType in expectedPluggableTypes)
						pluggables.ShouldContainInstanceOf(expectedPluggableType);
				}
			}
			return configuration;
		}
	}
}