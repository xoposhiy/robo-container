﻿using System;
using System.Linq;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Tests.Contracts
{
	[TestFixture]
	public class Contracts_Test
	{
		[Test]
		public void Can_use_Any_requirement()
		{
			var container = new Container(
				c =>
				{
					c.ForPluggable<PluggableWithContracts1>().DeclareContracts("a");
					c.ForPluggable<PluggableWithContracts2>().DeclareContracts("B", "c");
					c.ForPluggable<PluggableWithContracts3>().DeclareContracts();
					c.ForPluggable<PluggableWithContracts4>().DeclareContracts("B", "a", "C");
				}
				);
			var plugins = container.GetAll<IPluginWithContract>(Contract.Any);
			Assert.AreEqual(4, plugins.Count());
		}

		[Test]
		public void Can_use_Any_declaration()
		{
			var container = new Container(
				c => c.ForPluggable<PluggableWithContracts1>().DeclareContracts(Contract.Any));
			var plugin = container.Get<IPluginWithContract>("safasdfas", "asfd");
			Assert.AreEqual(plugin, container.Get<PluggableWithContracts1>());
			Assert.AreEqual(plugin, container.Get<PluggableWithContracts1>("srf234rqwerwerwer"));
			Assert.AreEqual(plugin, container.Get<PluggableWithContracts1>(Contract.Any));
		}

		[Test]
		public void Can_get_pluggables_with_any_contract()
		{
			var container = new Container();
			var plugins = container.GetAll<IPluginWithAttributes>();
			CollectionAssert.Contains(plugins, container.Get<HiddenPluggable>("hidden"));
			CollectionAssert.Contains(plugins, container.Get<FastHiddenPluggable>("fast"));
		}

		[Test]
		public void String_contracts_are_CaseInvariant()
		{
			var container = new Container();
			var plugin = container.Get<IPluginWithAttributes>("FaST", "hiDDen");
			Assert.IsInstanceOf<FastHiddenPluggable>(plugin);
		}

		[Test]
		public void Can_use_NameIsContractAttribute_for_contract_requirements()
		{
			var container = new Container(
				c =>
				{
					c.ForPlugin<string>().UseInstance("default");
					c.ForPlugin<string>().UseInstance("path", "referencesPath");
				}
					);
			Assert.AreEqual("path", container.Get<Root_With_NameIsContractAttribute>().referencesPath);
		}

		[Test]
		public void Can_mix_NameIsContractAttribute_with_other_contract_requirements()
		{
			var container = new Container(
				c =>
				{
					c.ForPlugin<string>().UseInstance("default");
					c.ForPlugin<string>().UseInstance("path", "referencesPath");
					c.ForPlugin<string>().UseInstance("local path", "referencesPath", "local");
					c.ForPlugin<string>().UseInstance("best local path", "referencesPath", "local", "best");
					c.ForPluggable<Root_With_NameIsContractAttribute_And_Other_Contracts>().Dependency("referencesPath").RequireContracts("best");
				}
					);
			Assert.AreEqual("best local path", container.Get<Root_With_NameIsContractAttribute_And_Other_Contracts>().referencesPath);
		}

		[Test]
		public void Can_use_NameIsContractAttribute_for_contract_declarations()
		{
			var container = new Container();
			Assert.IsInstanceOf<ClassWithNameIsContract>(container.Get<ClassWithNameIsContract>("classWithNameIsContract"));
			Assert.IsInstanceOf<ClassWithNameIsContract>(container.Get<IPlugin>("classWithNameIsContract"));
		}
		
		public interface IPlugin { }

		[NameIsContract]
		public class ClassWithNameIsContract : IPlugin
		{
			
		}
		
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
			container.Get<IPluginWithContract>(Contract.Default).ShouldBeInstanceOf<PluggableWithContracts2>();
		}

		[Test]
		public void DEFAULT_declared_contract_equivalent_to_no_contract()
		{
			var container = new Container(
				c =>
					{
						c.ForPluggable<PluggableWithContracts1>().DeclareContracts("1");
						c.ForPluggable<PluggableWithContracts2>().DeclareContracts(Contract.Default);
						c.ForPluggable<PluggableWithContracts3>().DeclareContracts("3");
						c.ForPluggable<PluggableWithContracts4>().DeclareContracts("4");
					});
			container.Get<IPluginWithContract>().ShouldBeInstanceOf<PluggableWithContracts2>();
			container.Get<IPluginWithContract>(Contract.Default).ShouldBeInstanceOf<PluggableWithContracts2>();
		}

		[Test]
		public void Contracts_without_code_infection()
		{
			var container = new Container();
			var hidden = container.Get<IPluginWithAttributes>(typeof(FastAndHiddenAttribute).Name);
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

	public class Root_With_NameIsContractAttribute
	{
		public readonly string referencesPath;

		public Root_With_NameIsContractAttribute([NameIsContract] string referencesPath)
		{
			this.referencesPath = referencesPath;
		}
	}
	
	public class Root_With_NameIsContractAttribute_And_Other_Contracts
	{
		public readonly string referencesPath;

		public Root_With_NameIsContractAttribute_And_Other_Contracts([NameIsContract] [RequireContract("local")]string referencesPath)
		{
			this.referencesPath = referencesPath;
		}
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
