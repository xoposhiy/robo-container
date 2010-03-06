using System;
using System.Linq;
using NUnit.Framework;
using RoboContainer.Core;

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
		public void With_can_override_pluggable_reuse()
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
		public void With_override_pluggable_Initializer()
		{
			// ReSharper disable AccessToModifiedClosure
			int parent_pluggable = 0;
			int parent_plugin = 0;
			int child_pluggable = 0;
			int child_plugin = 0;
			var container = new Container(
				c =>
					{
						c.ForPlugin<IFoo>().UsePluggable<Foo1>().SetInitializer(foo => parent_plugin++);
						c.ForPluggable<Foo1>().SetInitializer(foo => parent_pluggable--);
					});

			container.With(c => { }).Get<IFoo>();
			Assert.AreEqual(1, parent_plugin);
			Assert.AreEqual(-1, parent_pluggable);
			parent_plugin = 0;
			parent_pluggable = 0;
			container.With(
				c =>
					{
						c.ForPlugin<IFoo>().UsePluggable<Foo1>().SetInitializer(foo => child_plugin++);
						c.ForPluggable<Foo1>().SetInitializer(foo1 => child_pluggable--);
					}).Get<IFoo>();
			Assert.AreEqual(1, child_plugin);
			Assert.AreEqual(-1, child_pluggable);
			Assert.AreEqual(1, parent_plugin);
			Assert.AreEqual(-1, parent_pluggable);
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
			var container = new Container();
			var service = container.With(c => c.ForPluggable<Service>().Dependency("foo").UsePluggable<Foo1>()).Get<Service>();
			Assert.IsInstanceOf<Foo1>(service.foo);
		}
	}
}