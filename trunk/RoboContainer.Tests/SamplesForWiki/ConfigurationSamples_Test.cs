using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.SamplesForWiki
{
	[TestFixture]
	public class ConfigurationSamples_Test
	{
		[Test]
		public void FirstSample()
		{
			//[Configuration.FirstSample
			var container = new Container(
				(IContainerConfigurator c) =>
					{
						c.ForPlugin<IPlugin>().UsePluggable<Pluggable>();
						c.ForPluggable<Pluggable>().ReuseIt(ReusePolicy.Never);
						c.ScanLoadedAssemblies(assembly => (assembly.FullName + "").Contains("RoboContainer"));
						c.Logging.Disable();
					});
			//]
			Assert.IsInstanceOf<Pluggable>(container.Get<IPlugin>());
		}
	}

	public interface IPlugin
	{
	}

	public class Pluggable : IPlugin
	{
	}
}
