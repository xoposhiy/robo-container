using NUnit.Framework;
using RoboContainer.Impl;

namespace RoboContainer.Tests.Contracts
{
	[TestFixture]
	public class Contracts_Test
	{
		[Test]
		public void Can_declare_contracts_for_plugin()
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

		[Test]
		public void Can_inject_dependency_with_declared_contract()
		{
			var container = new Container(
				c =>
				{
					c.ForPluggable<PluggableWithContracts1>().DeclareContracts("fast");
					c.ForPluggable<PluginWithDeclarecContract>().Dependency("plugin").RequireContract("fast");

				});
			var plugin = container.Get<PluginWithDeclarecContract>();
			Assert.IsInstanceOf<PluggableWithContracts1>(plugin.Plugin);
		}

		[Test]
		public void Can_get_pluggable_with_required_contracts()
		{
			var container = new Container(
				c => c.ForPluggable<PluggableWithContracts1>().DeclareContracts("fast"));
			var plugin = container.Get<IPluginWithContract>(new NamedRequirement("fast"));
			Assert.IsInstanceOf<PluggableWithContracts1>(plugin);
		}
	}

	public class PluginWithDeclarecContract
	{
		public IPluginWithContract Plugin { get; set; }

		public PluginWithDeclarecContract(IPluginWithContract plugin)
		{
			Plugin = plugin;
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