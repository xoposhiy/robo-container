using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Tests.With
{
	[TestFixture]
	public class With_Test
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
			var container = new Container(c => c.ForPluggable<Singleton>().ReuseIt(ReusePolicy.Never));
			Assert.Throws<ContainerException>(() => container.Get<Singleton>());
			var singleton = container.With(c => c.Bind<IFoo, Foo1>()).Get<Singleton>();
			Assert.IsInstanceOf<Foo1>(singleton.foo);
			Assert.Throws<ContainerException>(() => container.Get<Singleton>());
		}

		[Test]
		public void With_can_override_reuse()
		{
			var container = new Container(c => c.Bind<IFoo, Foo1>(ReusePolicy.Never));
			IContainer child = container.With(c => c.ForPlugin<IFoo>().ReusePluggable(ReusePolicy.Always));
			Assert.AreSame(child.Get<IFoo>(), child.Get<IFoo>());
			Assert.AreNotSame(container.Get<IFoo>(), container.Get<IFoo>());
		}

		[Test]
		public void With_and_plugin_ReusePolicy()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().ReusePluggable(ReusePolicy.Never));
			
			IContainer child = container.With(c => c.Bind<IFoo, Foo1>(ReusePolicy.Always));
			Assert.AreSame(child.Get<IFoo>(), child.Get<IFoo>());
			
			child = container.With(c => c.Bind<IFoo, Foo1>());
			Assert.AreNotSame(child.Get<IFoo>(), child.Get<IFoo>());
		}

		[Test]
		public void With_and_plugin_Initializer()
		{
			// ReSharper disable AccessToModifiedClosure
			int counter = 0;
			var container = new Container(c => c.ForPlugin<IFoo>().SetInitializer(foo => counter++));

			container.With(c => c.Bind<IFoo, Foo1>()).Get<IFoo>();
			Assert.AreEqual(1, counter);

			container.With(c => c.ForPlugin<IFoo>().UsePluggable<Foo1>().SetInitializer(foo => counter+=10)).Get<IFoo>();
			Assert.AreEqual(11, counter);
			// ReSharper restore AccessToModifiedClosure
		}

	}
}