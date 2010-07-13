using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Tests
{
	public static class ContainerTestingExtensions
	{
		public static Action<IContainerConfigurator> Configure(
			this Action<IContainerConfigurator> configuration, Type pluginType,
			Type pluggableType)
		{
			return
				c =>
					{
						configuration(c);
						c.ForPlugin(pluginType).UsePluggable(pluggableType);
					};
		}

		public static Action<IContainerConfigurator> CheckThat(
			this Action<IContainerConfigurator> configuration, Type requestedPluginType,
			params Type[] expectedPluggableTypes)
		{
			var container = new Container(configuration);
			if(expectedPluggableTypes.Length == 1)
				container.Get(requestedPluginType).ShouldBeInstanceOf(expectedPluggableTypes.Single());
			else
			{
				IEnumerable<object> pluggables = container.GetAll(requestedPluginType);
				if(expectedPluggableTypes.Length == 0)
					pluggables.ShouldBeEmpty();
				else
					expectedPluggableTypes.ForEach(pluggables.ShouldContainInstanceOf);
			}
			Console.WriteLine(container.LastConstructionLog);
			return configuration;
		}
	}
}