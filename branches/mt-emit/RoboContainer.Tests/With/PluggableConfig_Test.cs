using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Tests.With
{
	[TestFixture]
	public class PluggableConfig_Test
	{
		public class Service
		{
			public readonly IFoo foo;

			public Service(IFoo foo)
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
		public void With_can_override_pluggable_reuse_Never()
		{
			var container = new Container(c => c.ForPluggable<Foo1>().ReuseIt(ReusePolicy.Never));
			IContainer child = container.With(c => c.ForPluggable<Foo1>().ReuseIt(ReusePolicy.Always));
			var expected = child.Get<Foo1>();
			Console.WriteLine(child.LastConstructionLog);
			var actual = child.Get<Foo1>();
			Console.WriteLine(child.LastConstructionLog);
			Assert.AreSame(expected, actual);
			Assert.AreNotSame(container.Get<Foo1>(), container.Get<Foo1>());
		}

		[Test]
		public void Can_not_override_reuse_policy_with_ReuseAlways_policy()
		{
			var container = new Container(c => c.ForPluggable<Foo1>().ReuseIt(ReusePolicy.Always));
			IContainer child = container.With(c => c.ForPluggable<Foo1>().ReuseIt(ReusePolicy.Never));
			Assert.Throws<ContainerException>(() => child.Get<Foo1>());
		}

		[Test]
		public void Can_not_override_initializer_with_ReuseAlways_policy()
		{
			var container = new Container(c => c.ForPluggable<Foo1>().ReuseIt(ReusePolicy.Always));
			IContainer child = container.With(c => c.ForPluggable<Foo1>().SetInitializer(foo => { }));
			Assert.Throws<ContainerException>(() => child.Get<Foo1>());
		}

		[Test]
		public void Can_override_reuse_policy_with_default_policy_in_parent_container()
		{
			var container = new Container();
			IContainer child = container.With(c => c.ForPluggable<Foo1>().ReuseIt(ReusePolicy.Never));
			Assert.AreNotSame(child.Get<Foo1>(), child.Get<Foo1>());
		}

		[Test]
		public void Can_override_initializer_with_default_reuse_policy_in_parent_container()
		{
			var container = new Container();
			int counter = 0;
			IContainer child = container.With(c => c.ForPluggable<Foo1>().SetInitializer(foo => counter++));
			Assert.AreSame(child.Get<Foo1>(), child.Get<Foo1>());
			Assert.AreEqual(1, counter);
		}

		[Test]
		public void InSameContainer_reuse_policy_dont_use_pluggables_from_parent_container()
		{
			var container = new Container(c => c.ForPluggable<Foo1>().ReuseIt(ReusePolicy.InSameContainer));
			IContainer child = container.With(c => { });
			Assert.AreSame(child.Get<Foo1>(), child.Get<Foo1>());
			Assert.AreSame(container.Get<Foo1>(), container.Get<Foo1>());
			Assert.AreNotSame(container.Get<Foo1>(), child.Get<Foo1>());
		}

		[Test]
		public void With_can_not_override_pluggable_reuse_InSameContainerHierarchy()
		{
			var container = new Container(c => c.ForPluggable<Foo1>().ReuseIt(ReusePolicy.InSameContainer));
			IContainer child = container.With(c => c.ForPluggable<Foo1>().ReuseIt(ReusePolicy.Never));
			Assert.AreNotSame(child.Get<Foo1>(), child.Get<Foo1>());
			Assert.AreSame(container.Get<Foo1>(), container.Get<Foo1>());
		}

		[Test]
		public void With_override_pluggable_Initializer()
		{
			// ReSharper disable AccessToModifiedClosure
			int parent_pluggable = 0;
			int parent_plugin = 0;
			var container = new Container(
				c =>
					{
						c.ForPlugin<IFoo>().UsePluggable<Foo1>().SetInitializer(foo => parent_plugin++);
						c.ForPluggable<Foo1>().ReuseIt(ReusePolicy.Never).SetInitializer(
							foo => { parent_pluggable++;
							       	//Debugger.Break();
							}
							);
					});

			container.With(c => { }).Get<IFoo>();
			Assert.AreEqual(1, parent_plugin);
			Assert.AreEqual(1, parent_pluggable);
			parent_plugin = 0;
			parent_pluggable = 0;
			int child_pluggable = 0;
			int child_plugin = 0;
			var childContainer = container.With(
				c =>
					{
						c.ForPlugin<IFoo>().SetInitializer(foo => child_plugin++);
						c.ForPluggable<Foo1>().SetInitializer(
							foo => child_pluggable++
							);
					});
			childContainer.Get<IFoo>();
			Assert.AreEqual(1, child_plugin);
			Assert.AreEqual(1, child_pluggable);
			Assert.AreEqual(0, parent_plugin);
			Assert.AreEqual(0, parent_pluggable);
			// ReSharper restore AccessToModifiedClosure
		}

		[Test]
		public void With_can_expand_contracts_requirements()
		{
			var container = new Container(
				c => c.ForPlugin<IFoo>().ReusePluggable(ReusePolicy.Never).RequireContracts("contract"));
			var childContainer = container.With(c => c.ForPluggable<Foo1>().DeclareContracts("contract"));
			var childFooes = childContainer.GetAll<IFoo>();
			Assert.AreEqual(1, childFooes.Count());
			var parentFooes = container.GetAll<IFoo>();
			Assert.AreEqual(0, parentFooes.Count());
		}

		[Test]
		public void With_refine_dependency_configurations()
		{
			var container = new Container(c => c.ForPluggable<Service>().ReuseIt(ReusePolicy.Never));
			var service = container.With(c => c.ForPluggable<Service>().Dependency("foo").UsePluggable<Foo1>()).Get<Service>();
			Assert.IsInstanceOf<Foo1>(service.foo);
		}

		[Test]
		public void With_preserves_original_dependency_configurations()
		{
			var container = new Container(c => c.ForPluggable<Service>().ReuseIt(ReusePolicy.Never).Dependency("foo").UsePluggable<Foo2>());
			var service = container.With(c => { }).Get<Service>();
			Assert.IsInstanceOf<Foo2>(service.foo);
		}
	}
}