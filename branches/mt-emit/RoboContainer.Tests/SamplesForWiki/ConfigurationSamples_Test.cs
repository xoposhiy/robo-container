using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoboContainer.Core;

// ReSharper disable RedundantLambdaParameterType
namespace RoboContainer.Tests.SamplesForWiki
{
	[TestFixture]
	public class ConfigurationSamples_Test
	{
		[Test]
		public void ConfigurePlugin()
		{
			//[Configuration.Plugin
			var explicitlySpecifiedPluggable = new Pluggable();
			var container = new Container(
				c =>
				c.ForPlugin<IPlugin>()
					.UseInstance(explicitlySpecifiedPluggable) //автоматика отключилась
					.UsePluggablesAutosearch(true)  //но мы ее принудительно включили обратно
					.ReusePluggable(ReusePolicy.Never)
				);
			IEnumerable<IPlugin> pluggables = container.GetAll<IPlugin>();
			Console.WriteLine(container.LastConstructionLog); //hide
			CollectionAssert.Contains(pluggables, explicitlySpecifiedPluggable);
			Assert.AreEqual(2, pluggables.Count()); // вторую реализацию нашла автоматика.
			//]
		}

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