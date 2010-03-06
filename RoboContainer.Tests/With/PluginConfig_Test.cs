using System;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Impl;
using System.Linq;
using RoboContainer.Infection;

namespace RoboContainer.Tests.With
{
	[TestFixture]
	public class PluginConfig_Test
	{
		public class Singleton
		{
			public readonly IFoo foo;

			public Singleton(IFoo foo)
			{
				this.foo = foo;
			}
		}

		public interface IFoo
		{
		}

		public class Foo1 : IFoo
		{
		}

		public class Foo2 : IFoo
		{
		}

		[Test]
		public void Singletons_of_parent_cant_have_child_dependencies()
		{
			Assert.Throws<ContainerException>(
				() => new Container().With(c => c.Bind<IFoo, Foo1>()).Get<Singleton>()
				);
		}

		[Test]
		public void With_refines_parent_configuration()
		{
			var container = new Container();
			Assert.Throws<ContainerException>(() => container.Get<IFoo>());
			var foo1 = container.With(c => c.Bind<IFoo, Foo1>()).Get<IFoo>();
			Assert.IsInstanceOf<Foo1>(foo1);
			Assert.Throws<ContainerException>(() => container.Get<IFoo>());
		}

		[Test]
		public void With_overrides_parent_container_even_indirect()
		{
			var container = new Container(c => c.ForPlugin<Singleton>().ReusePluggable(ReusePolicy.Never));
			Assert.Throws<ContainerException>(() => container.Get<Singleton>());
			var singleton = container.With(c => c.Bind<IFoo, Foo1>()).Get<Singleton>();
			Assert.IsInstanceOf<Foo1>(singleton.foo);
			Assert.Throws<ContainerException>(() => container.Get<Singleton>());
		}

		[Test]
		public void With_override_plugin_ReusePolicy_1()
		{
			var container = new Container(c => c.Bind<IFoo, Foo1>(ReusePolicy.Never));
			IContainer child = container.With(c => c.ForPlugin<IFoo>().ReusePluggable(ReusePolicy.Always));
			Assert.AreSame(child.Get<IFoo>(), child.Get<IFoo>());
			Assert.AreNotSame(container.Get<IFoo>(), container.Get<IFoo>());
		}

		[Test]
		public void With_override_plugin_ReusePolicy_2()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().ReusePluggable(ReusePolicy.Never));
			
			IContainer child = container.With(c => c.Bind<IFoo, Foo1>(ReusePolicy.Always));
			Assert.AreSame(child.Get<IFoo>(), child.Get<IFoo>());
			
			child = container.With(c => c.Bind<IFoo, Foo1>());
			Assert.AreNotSame(child.Get<IFoo>(), child.Get<IFoo>());
		}

		[Test]
		public void With_override_plugin_Initializer()
		{
			// ReSharper disable AccessToModifiedClosure
			int counter = 0;
			var container = new Container(c => c.ForPlugin<IFoo>().SetInitializer(foo => counter++));

			container.With(c => c.Bind<IFoo, Foo1>()).Get<IFoo>();
			Assert.AreEqual(1, counter);

			container.With(c => c.ForPlugin<IFoo>().UsePluggable<Foo1>().SetInitializer(foo => counter+=10)).Get<IFoo>();
			Assert.AreEqual(12, counter);
			// ReSharper restore AccessToModifiedClosure
		}

		[Test]
		public void With_expands_explicitly_specified_pluggables()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().UseInstance(new Foo1()));
			var parentFooes = container.GetAll<IFoo>();
			var childFooes = container.With(c => c.ForPlugin<IFoo>().UsePluggable<Foo2>()).GetAll<IFoo>();
			Assert.AreEqual(2, childFooes.Count());
			Assert.AreEqual(1, parentFooes.Count());
		}

		[Test]
		public void With_can_turn_on_autosearch()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().UseInstance(new Foo1()));
			var parentFooes = container.GetAll<IFoo>();
			var childContainer = container.With(c => c.ForPlugin<IFoo>().UseAutoFoundPluggables());
			var childFooes = childContainer.GetAll<IFoo>();
			Console.WriteLine(childContainer.LastConstructionLog);
			Console.WriteLine(container.LastConstructionLog);
			Assert.AreEqual(3, childFooes.Count());
			Assert.AreEqual(1, parentFooes.Count());
		}

		[Test]
		public void With_can_turn_off_autosearch()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().UseInstance(new Foo1()).UseAutoFoundPluggables());
			var parentFooes = container.GetAll<IFoo>();
			var childContainer = container.With(c => c.ForPlugin<IFoo>().DontUseAutoFoundPluggables());
			var childFooes = childContainer.GetAll<IFoo>();
			Console.WriteLine(childContainer.LastConstructionLog);
			Console.WriteLine(container.LastConstructionLog);
			Assert.AreEqual(1, childFooes.Count());
			Assert.AreEqual(3, parentFooes.Count());
		}

		[Test]
		public void With_can_expand_contracts_requirements()
		{
			var container = new Container(c => c.ForPlugin<IContractedFoo>().RequireContracts("for_parent"));
			var parentFooes = container.GetAll<IContractedFoo>();
			var childContainer = container.With(c => c.ForPlugin<IContractedFoo>().RequireContracts("for_child"));
			var childFooes = childContainer.GetAll<IContractedFoo>();
			Assert.AreEqual(2, parentFooes.Count());
			Assert.AreEqual(1, childFooes.Count());
		}

		[Test]
		public void With_can_stop_use_some_pluggables()
		{
			var container = new Container();
			var parentFooes = container.GetAll<IFoo>();
			var childContainer = container.With(c => c.ForPlugin<IFoo>().DontUse<Foo1>());
			var childFooes = childContainer.GetAll<IFoo>();
			Assert.AreEqual(2, parentFooes.Count());
			Assert.AreEqual(1, childFooes.Count());
		}

		public interface IContractedFoo
		{
		}

		[DeclareContract("for_parent")]
		public class ContractedFoo1 : IContractedFoo { }

		[DeclareContract("for_child")]
		public class ContractedFoo2 : IContractedFoo { }

		[DeclareContract("for_child", "for_parent")]
		public class ContractedFoo3 : IContractedFoo { }

		public class ContractedFoo4 : IContractedFoo { }
	}

}