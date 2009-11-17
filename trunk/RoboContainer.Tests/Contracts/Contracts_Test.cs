using NUnit.Framework;

namespace RoboContainer.Tests.Contracts
{
	[TestFixture]
	public class Contracts_Test
	{
		[Test]
		public void Test()
		{
			var container = new Container(
				c =>
					{
						c.ForPlugin<IPluginWithContract>().RequireContracts("a", "B");
						c.ForPluggable<PluggableWithContracts1>().DeclareContracts("a");
						c.ForPluggable<PluggableWithContracts2>().DeclareContracts("B", "c");
						c.ForPluggable<PluggableWithContracts3>().DeclareContracts();
						c.ForPluggable<PluggableWithContracts4>().DeclareContracts("B", "a", "C");
					}
				);
			Assert.IsInstanceOf<PluggableWithContracts4>(container.Get<IPluginWithContract>());
		}
	}

	public class PluggableWithContracts4 : IPluginWithContract
	{
	}

	public class PluggableWithContracts3 : IPluginWithContract
	{
	}

	public class PluggableWithContracts2 : IPluginWithContract
	{
	}

	public class PluggableWithContracts1 : IPluginWithContract
	{
	}

	public interface IPluginWithContract
	{
	}
}