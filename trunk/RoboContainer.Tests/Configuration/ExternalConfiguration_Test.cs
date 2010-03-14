using System.IO;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.Configuration
{
	[TestFixture]
	public class ExternalConfiguration_Test
	{
		[Test]
		public void TestCase()
		{
			{
				//[XmlConfiguration.ConfigByCode
				var container = new Container(
					c =>
					{
						c.ForPlugin<IComponent>()
							.UsePluggable<Component1>()
							.UseAutoFoundPluggables()
							.DontUse<Component2>()
							.DontUse<Component3>()
							.DontUse<Component4>()
							.ReusePluggable(ReusePolicy.Never);
						c.ForPluggable<Component1>().DontUseIt();
						c.ForPluggable<Component4>()
							.ReuseIt(ReusePolicy.Never)
							.UseConstructor(typeof(IComponent))
							.Dependency("comp").UsePluggable<Component2>();
					}
					);
				//]
				Check(container);
			}
			{
				//[XmlConfiguration.ConfigByXmlFile
				var container = new Container(c => c.ConfigureBy.XmlFile("Configuration\\ConfigSample.xml"));
				//]
				File.Copy("Configuration\\ConfigSample.xml", "Configuration\\XmlConfiguration.Config.xml.txt", true);
				Check(container);
			}
		}

		private static void Check(Container container)
		{
			Assert.AreNotSame(container.Get<IComponent>(), container.Get<IComponent>());
			Assert.IsInstanceOf<Component1>(container.Get<IComponent>());
			var component4 = container.Get<Component4>();
			Assert.IsInstanceOf<Component2>(component4.comp);
			Assert.AreNotSame(component4, container.Get<Component4>());
			Assert.AreSame(container.Get<Component3>(), container.Get<Component3>());
		}
	}

	public interface IComponent
	{
	}

	public class Component1 : IComponent
	{
	}

	public class Component2 : IComponent
	{
	}

	public class Component3 : IComponent
	{
	}

	public class Component4 : IComponent
	{
		public readonly IComponent comp;

		public Component4()
		{
		}

		public Component4(IComponent comp)
		{
			this.comp = comp;
		}
	}
}