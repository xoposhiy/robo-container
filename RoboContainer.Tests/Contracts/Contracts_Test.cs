using System;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Infection;

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
		public void Can_get_pluggable_with_required_contracts()
		{
			var container = new Container(
				c => c.ForPluggable<PluggableWithContracts1>().DeclareContracts("fast"));
			var plugin = container.Get<IPluginWithContract>("fast");
			Console.WriteLine(container.LastConstructionLog);
			Assert.IsInstanceOf<PluggableWithContracts1>(plugin);
		}

		[Test]
		public void Can_inject_dependency_with_required_contract()
		{
			var container = new Container(
				c =>
					{
						c.ForPluggable<PluggableWithContracts1>().DeclareContracts("fast");
						c.ForPluggable<PluginWithDeclaredContract>().Dependency("plugin").RequireContracts("fast");
					});
			var plugin = container.Get<PluginWithDeclaredContract>();
			Assert.IsInstanceOf<PluggableWithContracts1>(plugin.Plugin);
		}

		[Test]
		public void Dependency_injection_with_required_contract_considers_plugin_contract_requirements()
		{
			var container = new Container(
				c =>
					{
						c.ForPlugin<IPluginWithContract>().RequireContracts("hidden");
						c.ForPluggable<PluggableWithContracts1>().DeclareContracts("fast", "hidden");
						c.ForPluggable<PluggableWithContracts2>().DeclareContracts("fast");
						c.ForPluggable<PluggableWithContracts3>().DeclareContracts("hidden");
						c.ForPluggable<PluginWithDeclaredContract>().Dependency("plugin").RequireContracts("fast");
					});
			var plugin = container.Get<PluginWithDeclaredContract>();
			Assert.IsInstanceOf<PluggableWithContracts1>(plugin.Plugin);
		}

		[Test]
		public void Contract_requirements_can_be_specified_with_attributes()
		{
			var container = new Container();
			var root = container.Get<Root>();
			Assert.IsInstanceOf<FastHiddenPluggable>(root.Plugin);
		}

		[Test]
		public void No_declared_contracts_equivalent_to_DEFAULT_contract()
		{
			var container = new Container(
				c =>
					{
						c.ForPluggable<PluggableWithContracts1>().DeclareContracts("1");
						c.ForPluggable<PluggableWithContracts2>().DeclareContracts();
						c.ForPluggable<PluggableWithContracts3>().DeclareContracts("3");
						c.ForPluggable<PluggableWithContracts4>().DeclareContracts("4");
					});
			container.Get<IPluginWithContract>().ShouldBeInstanceOf<PluggableWithContracts2>();
			container.Get<IPluginWithContract>(ContractRequirement.Default).ShouldBeInstanceOf<PluggableWithContracts2>();
		}

		[Test]
		public void DEFAULT_declared_contract_equivalent_to_no_contract()
		{
			var container = new Container(
				c =>
					{
						c.ForPluggable<PluggableWithContracts1>().DeclareContracts("1");
						c.ForPluggable<PluggableWithContracts2>().DeclareContracts(ContractDeclaration.Default);
						c.ForPluggable<PluggableWithContracts3>().DeclareContracts("3");
						c.ForPluggable<PluggableWithContracts4>().DeclareContracts("4");
					});
			container.Get<IPluginWithContract>().ShouldBeInstanceOf<PluggableWithContracts2>();
			container.Get<IPluginWithContract>(ContractRequirement.Default).ShouldBeInstanceOf<PluggableWithContracts2>();
		}

		[Test]
		public void Contracts_without_code_infection()
		{
			var container = new Container();
			var hidden = container.Get<IPluginWithAttributes>(typeof(FastAndHiddenAttribute));
			Assert.IsInstanceOf<FastHiddenPluggable>(hidden);
			Assert.IsInstanceOf<FastHiddenPluggable>(container.Get<Framework>().Plugin);
			Assert.IsInstanceOf<BestFramework>(container.Get<IFramework>());
		}
	}

	[RequireContract("hidden")]
	public interface IPluginWithAttributes
	{
	}

	[BestFramework]
	interface IFramework{}

	[MeansInjectionContract]
	internal class BestFrameworkAttribute : Attribute
	{
	}

	[BestFramework]
	public class BestFramework : IFramework
	{
		
	}

	public class Framework : IFramework
	{
		public IPluginWithAttributes Plugin { get; set; }

		public Framework([FastAndHidden, NotNull] IPluginWithAttributes plugin)
		{
			Plugin = plugin;
		}
	}

	public class NotNullAttribute : Attribute
	{
	}

	[MeansInjectionContract]
	public class FastAndHiddenAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class MeansInjectionContract : Attribute
	{
	}

	[DeclareContract("hidden")]
	public class HiddenPluggable : IPluginWithAttributes
	{
	}

	[DeclareContract("hidden")]
	[DeclareContract("fast")]
	[FastAndHidden]
	public class FastHiddenPluggable : IPluginWithAttributes
	{
	}

	[DeclareContract("fast")]
	public class FastPluggable : IPluginWithAttributes
	{
	}

	public class Root
	{
		public IPluginWithAttributes Plugin { get; private set; }

		public Root([RequireContract("fast")] IPluginWithAttributes plugin)
		{
			Plugin = plugin;
		}
	}

	public class PluginWithDeclaredContract
	{
		public IPluginWithContract Plugin { get; set; }

		public PluginWithDeclaredContract(IPluginWithContract plugin)
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
